using System;
using System.Collections.Generic;
using System.IO;

namespace SGF.IO
{
    /// <summary>
    /// Contains helpers for working with files paths 
    /// </summary>
    public static class SGFPath
    {
        /// <summary>
        /// The separator used for windows paths 
        /// </summary>
        public const char WINDOWS_SEPARATOR = '\\';

        /// <summary>
        /// The separator used in Linux paths 
        /// </summary>
        public const char LINUX_SEPARATOR = '/';

        /// <summary>
        /// The list of path separators used on all platforms 
        /// </summary>
        public static readonly IReadOnlyCollection<char> PathSeparators;

        static SGFPath()
        {
            PathSeparators = new[] { WINDOWS_SEPARATOR, LINUX_SEPARATOR };
        }

        /// <summary>
        /// Given a path we return back it's root 
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string? GetPathRoot(string path)
        {
            if (path == null)
            {
                return null;
            }

            if (HasWinRoot(path))
            {
                return path.Substring(0, 2);
            }

            return HasLinuxRoot(path) ? LINUX_SEPARATOR.ToString() : null;
        }

        /// <summary>
        /// Determines whether the path is rooted to windows
        /// </summary>
        /// <param name="path">The path.</param>
        public static bool HasWinRoot(string? path)
        {
            return path?.Length > 1 && char.IsLetter(path[0]) && path[1] == ':';
        }

        /// <summary>
        /// Determines whether a given path is rooted to Linux.
        /// </summary>
        /// <param name="path">The path.</param>
        public static bool HasLinuxRoot(string? path)
        {
            return path?.Length > 0 && path[0] == LINUX_SEPARATOR;
        }

        /// <summary>
        /// Determines whether the specified path is rooted.
        /// </summary>
        /// <param name="path">The path you want to check.</param>
        public static bool IsRooted(string? path)
        {
            return HasWinRoot(path) || HasLinuxRoot(path);
        }

        /// <summary>
        /// Takes in a string path and resolve all directory navigators ('.', '..') and returns
        /// back a constant format with all slashes facing the same direction. 
        /// </summary>
        /// <param name="path">The path you want to normalize</param>
        /// <param name="separator">The optional separator you want to use</param>
        /// <returns></returns>
        public static string? Normalize(string path, char? separator = null)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            path = Environment.ExpandEnvironmentVariables(path);
            separator ??= GetSeparator(path);
            string? root = GetPathRoot(path);
            string tail = root == null ? path : path.Substring(root.Length);
            string[] components = tail.Split((char[])PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            string[] normalized = new string[components.Length];
            int index = 0;
            bool isRooted = IsRooted(path);

            foreach (string component in components)
            {
                switch (component)
                {
                    case ".":
                        continue;
                    case "..":
                        if (index == 0 || normalized[index - 1] == "..")
                        {
                            if (isRooted)
                            {
                                throw new ArgumentException($"Cannot normalize '{path}' beyond path root.");
                            }
                        }
                        else
                        {
                            index--;
                            continue;
                        }
                        break;
                }

                normalized[index] = component;
                index++;
            }

            return Combine(root, string.Join(separator.ToString(), normalized, 0, index), separator);
        }

        /// <summary>
        /// Trims the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public static string? Trim(string? path)
        {
            return path == null
                ? string.Empty
                : HasLinuxRoot(path)
                ? path
                : path.TrimEnd((char[])PathSeparators);
        }

        /// <summary>
        /// Combines two paths into one.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The result of the combination</returns>
        public static string? Combine(string? left, string? right, char? separator = null)
        {
            left = Trim(left);
            right = Trim(right);

            if (IsRooted(right))
            {
                throw new ArgumentException("The second path must not be rooted to be combined", nameof(right));
            }

            if (string.IsNullOrWhiteSpace(left))
            {
                return right;
            }

            if (string.IsNullOrWhiteSpace(right))
            {
                return HasWinRoot(left) ? $@"{left}{WINDOWS_SEPARATOR}" : left;
            }

            if (separator is null)
            {
                separator = GetSeparator(left!);
            }
            else
            {
                return Join(left!, right!, separator.Value);
            }

            if (HasWinRoot(left))
            {
                return Join(left!, right!, WINDOWS_SEPARATOR);
            }

            return HasLinuxRoot(left) ? Join(left!, right!, LINUX_SEPARATOR) : Join(left!, right!, separator.Value);
        }

        /// <summary>
        /// Gets the separator char based off the rooted directory, if the path is not rooted we just return the default for the current platform.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">path - We are unable to get the separator from a null path</exception>
        public static char GetSeparator(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "We are unable to get the separator from a null path");
            }

            if (HasWinRoot(path))
            {
                return WINDOWS_SEPARATOR;
            }

            return HasLinuxRoot(path) ? LINUX_SEPARATOR : Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Joins two paths together and adds the separator if it's not already added.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        private static string Join(string left, string right, char separator)
        {
            int length = left.Length;
            char[] result = new char[left.Length + right.Length + 1];


            if (left[length - 1] == separator)
            {
                length--;
            }

            left.CopyTo(0, result, 0, length);

            result[length] = separator;
            length++;

            int startIndex = 0;
            if (right[0] == separator)
            {
                startIndex++;
            }
            right.CopyTo(startIndex, result, length, right.Length - startIndex);
            length += right.Length - startIndex;

            return new string(result, 0, length);
        }

    }
}
