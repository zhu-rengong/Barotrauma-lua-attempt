// @ts-check

const { info, error, getInput, setFailed } = require("@actions/core");
const { getOctokit, context } = require("@actions/github");
const { setTimeout: sleep } = require("timers/promises");
const { basename } = require("node:path");
const { statSync, createReadStream } = require("node:fs");
const { globSync } = require("glob");

const RELEASE_TYPES = new Map([
  [
    "latest",
    {
      tag: "latest",
      tagMessage: "Latest build",
      releaseName: "Automatic build",
      releaseMessage: "Automatic build",
    },
  ],
]);

/**
 * @param {unknown} err
 */
function normalizeError(err) {
  if (err instanceof Error) {
    return err;
  }
  return new Error(`unknown error: ${JSON.stringify(err)}`);
}

async function run() {
  const type = RELEASE_TYPES.get(
    getInput("type", {
      required: true,
    }),
  );
  if (!type) {
    throw new Error(`invalid release type`);
  }

  const files = getInput("files", {
    required: false,
  }).split("\n");

  const octokit = getOctokit(
    getInput("token", {
      required: true,
    }),
    {
      request: {
        timeout: 30000,
      },
    },
  );

  const {
    repo: { owner, repo },
    sha,
  } = context;

  const releases = await octokit.paginate("GET /repos/:owner/:repo/releases", {
    owner,
    repo,
  });
  info(`deleting releases associated with tag '${type.tag}'`);
  for (const release of releases) {
    if (release.tag_name !== type.tag) {
      continue;
    }
    const release_id = release.id;
    info(`deleting release ${release_id}`);
    await octokit.rest.repos.deleteRelease({ owner, repo, release_id });
  }

  try {
    info(`attempting to update existing tag`);
    await octokit.rest.git.updateRef({
      owner,
      repo,
      ref: `tags/${type.tag}`,
      sha,
      force: true,
    });
    info(`successfully updated tag`);
  } catch (err) {
    error(normalizeError(err));
    info(`creating tag`);
    await octokit.rest.git.createTag({
      owner,
      repo,
      tag: type.tag,
      message: type.tagMessage,
      object: sha,
      type: "commit",
    });
    info(`successfully created tag`);
  }

  info(`creating release`);
  const release = await octokit.rest.repos.createRelease({
    owner,
    repo,
    name: type.releaseName,
    body: type.releaseMessage,
    tag_name: type.tag,
    target_commitish: sha,
  });
  const release_id = release.data.id;

  for (const file of globSync(files)) {
    const size = statSync(file).size;
    const name = basename(file);

    await runWithRetry(async function () {
      // We can't overwrite assets, so remove existing ones from previous the attempt.
      let assets = await octokit.rest.repos.listReleaseAssets({
        owner,
        repo,
        release_id,
      });
      for (const asset of assets.data) {
        if (asset.name === name) {
          info(`deleting existing asset from previous attempt: ${name}`);
          const asset_id = asset.id;
          await octokit.rest.repos.deleteReleaseAsset({
            owner,
            repo,
            asset_id,
          });
        }
      }

      info(`uploading file: ${file}`);
      const headers = {
        "content-length": size,
        "content-type": "application/octet-stream",
      };
      const data = createReadStream(file);
      await octokit.rest.repos.uploadReleaseAsset({
        // @ts-ignore: if only they could get their types right...
        data,
        headers,
        name,
        url: release.data.upload_url,
      });
    });
  }
}

/**
 * @param {() => Promise<any>} f
 */
async function runWithRetry(f) {
  const retries = 10;
  const maxDelay = 4000;
  let delay = 1000;

  for (let i = 0; i < retries; i++) {
    try {
      await f();
      break;
    } catch (err) {
      if (i === retries - 1) {
        throw err;
      }

      error(normalizeError(err));
      const currentDelay = Math.round(Math.random() * delay);
      info(`sleeping ${currentDelay} ms`);
      await sleep(currentDelay);
      delay = Math.min(delay * 2, maxDelay);
    }
  }
}

runWithRetry(run).catch((_err) => {
  const err = normalizeError(_err);
  error(err);
  setFailed(err.message);
});
