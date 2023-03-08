string.startsWith = function(str, start)
    return string.sub(str, 1, string.len(start)) == start
end

string.endsWith = function(str, ending)
    return ending == "" or string.sub(str, -string.len(ending)) == ending
end