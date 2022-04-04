
using System;
using System.IO;
using UnityEngine;

namespace BsDiff
{
    public static class BsTool
    {
        /// <summary>
        /// Apply a Diff from <paramref name="patchFile"/> to <paramref name="oldFile"/> resulting in the creation of <paramref name="newFile"/>
        /// </summary>
        /// <param name="oldFile">File to be patched using <paramref name="patchFile"/></param>
        /// <param name="newFile">File to be created from patching <paramref name="oldFile"/> with <paramref name="patchFile"/></param>
        /// <param name="patchFile">Diff file used for transforming <paramref name="oldFile"/> into <paramref name="newFile"/></param>
        public static void Patch(string oldFile, string newFile, string patchFile)
        {
            try
            {
                using (FileStream input = new FileStream(oldFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream output = new FileStream(newFile, FileMode.Create))
                    BinaryPatchUtility.Apply(input, () => new FileStream(patchFile, FileMode.Open, FileAccess.Read, FileShare.Read), output);
            }
            catch (FileNotFoundException ex)
            {
                Debug.LogError($"Could not open '{ex.FileName}'.\r\n{ex.Message}");
            }
        }

        /// <summary>
        /// Create a Diff from <paramref name="oldFile"/> to <paramref name="newFile"/> resulting in the creation of <paramref name="patchFile"/>
        /// </summary>
        /// <param name="oldFile">File that <paramref name="patchFile"/> will transform into <paramref name="newFile"/></param>
        /// <param name="newFile">File that is compared to <paramref name="oldFile"/> to create <paramref name="patchFile"/></param>
        /// <param name="patchFile">Diff file to be created for transforming <paramref name="oldFile"/> into <paramref name="newFile"/></param>
        public static void CreateDiff(string oldFile, string newFile, string patchFile)
        {
            try
            {
                using (FileStream output = new FileStream(patchFile, FileMode.Create))
                    BinaryPatchUtility.Create(File.ReadAllBytes(oldFile), File.ReadAllBytes(newFile), output);
            }
            catch (FileNotFoundException ex)
            {
                Debug.LogError($"Could not open '{ex.FileName}'.\r\n{ex.Message}");
            }
        }
    }
}
