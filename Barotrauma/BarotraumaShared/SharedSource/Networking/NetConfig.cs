﻿using Microsoft.Xna.Framework;
using System;

namespace Barotrauma.Networking
{
    static class NetConfig
    {
        public const int DefaultPort = 27015;
        public const int DefaultQueryPort = 27016;

        public static int MaxPlayers = 256;

        public static int ServerNameMaxLength = 60;
        public static int ServerMessageMaxLength = 2000;

        public const float MaxPhysicsBodyVelocity = 64.0f;
        public const float MaxPhysicsBodyAngularVelocity = 16.0f;

        public static float MaxHealthUpdateInterval = 2.0f;
        public static float MaxHealthUpdateIntervalDead = 10.0f;

        public static float HighPrioCharacterPositionUpdateDistance = 1000.0f;
        public static float LowPrioCharacterPositionUpdateDistance = 10000.0f;
        public static float HighPrioCharacterPositionUpdateInterval = 0.0f;
        public static float LowPrioCharacterPositionUpdateInterval = 1.0f;

        //this should be higher than LowPrioCharacterPositionUpdateInterval,
        //otherwise the clients may freeze characters even though the server hasn't actually stopped sending position updates
        public static float FreezeCharacterIfPositionDataMissingDelay = 2.0f;
        public static float DisableCharacterIfPositionDataMissingDelay = 3.5f;

        public static float DeleteDisconnectedTime = 20.0f;

        public static float ItemConditionUpdateInterval = 0.15f;
        public static float LevelObjectUpdateInterval = 0.5f;
        public static float HullUpdateInterval = 0.5f;
        public static float SparseHullUpdateInterval = 5.0f;
        public static float HullUpdateDistance = 20000.0f;

        public static int MaxEventPacketsPerUpdate = 4;

        /// <summary>
        /// How long the server waits for the clients to get in sync after the round has started before kicking them
        /// </summary>
        public static float RoundStartSyncDuration = 60.0f;

        /// <summary>
        /// How long the server keeps events that everyone currently synced has received
        /// </summary>
        public static float EventRemovalTime = 15.0f;

        /// <summary>
        /// If a client hasn't received an event that has been succesfully sent to someone within this time, they get kicked
        /// </summary>
        public static float OldReceivedEventKickTime = 10.0f;

        /// <summary>
        /// If a client hasn't received an event after this time, they get kicked
        /// </summary>
        public static float OldEventKickTime = 30.0f;

        /// <summary>
        /// Interpolates the positional error of a physics body towards zero.
        /// </summary>
        public static Vector2 InterpolateSimPositionError(Vector2 simPositionError, float? smoothingFactor = null)
        {
            float lengthSqr = simPositionError.LengthSquared();
            //correct immediately if the error is very large
            if (lengthSqr > 100.0f) { return Vector2.Zero; }
            float positionSmoothingFactor = smoothingFactor ?? MathHelper.Lerp(0.95f, 0.8f, MathHelper.Clamp(lengthSqr, 0.0f, 1.0f));
            return simPositionError *= positionSmoothingFactor;
        }

        /// <summary>
        /// Interpolates the rotational error of a physics body towards zero.
        /// </summary>
        public static float InterpolateRotationError(float rotationError)
        {
            //correct immediately if the error is very large
            if (rotationError > MathHelper.TwoPi) { return 0.0f; }
            float rotationSmoothingFactor = MathHelper.Lerp(0.95f, 0.8f, Math.Min(Math.Abs(rotationError), 1.0f));
            return rotationError *= rotationSmoothingFactor;
        }

        /// <summary>
        /// Interpolates the cursor position error towards zero.
        /// </summary>
        public static Vector2 InterpolateCursorPositionError(Vector2 cursorPositionError)
        {
            float lengthSqr = cursorPositionError.LengthSquared();
            //correct immediately if the error is very large
            if (lengthSqr > 1000.0f) { return Vector2.Zero; }
            return cursorPositionError *= 0.7f;
        }

        public static Vector2 Quantize(Vector2 value, float min, float max, int numberOfBits)
        {
            return new Vector2(
                Quantize(value.X, min, max, numberOfBits), 
                Quantize(value.Y, min, max, numberOfBits));
        }

        public static float Quantize(float value, float min, float max, int numberOfBits)
        {
            float step = (max - min) / (1 << (numberOfBits + 1));
            if (Math.Abs(value) < step + 0.00001f)
            {
                return 0.0f;
            }

            return MathUtils.RoundTowardsClosest(MathHelper.Clamp(value, min, max), step);
        }
    }
}
