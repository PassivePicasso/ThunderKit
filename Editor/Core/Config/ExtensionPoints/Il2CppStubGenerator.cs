using System;
using System.Collections.Generic;
using ThunderKit.Core.Data;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// Extension point that recovers an IL2CPP game's managed type metadata into
    /// <em>stub</em> assemblies: full type / field / attribute metadata with empty
    /// method bodies, suitable for the Editor to author against. ThunderKit ships
    /// the seam, not a bundled dumper - implement this in an assembly marked with
    /// <see cref="ImportExtensionsAttribute"/> to plug a specific generator
    /// (e.g. Cpp2IL or Il2CppDumper) into the IL2CPP import path.
    ///
    /// Generators are discovered and ordered by <see cref="ImportExtension.Priority"/>
    /// (descending). <see cref="ImportIl2CppStubs"/> uses the highest-priority
    /// generator whose <see cref="CanGenerate"/> returns true.
    /// </summary>
    [Serializable]
    public abstract class Il2CppStubGenerator : ImportExtension
    {
        /// <summary>
        /// Whether this generator is able to produce stubs for the configured game.
        /// Implementations can inspect the game's files (metadata version, presence
        /// of a readable global-metadata.dat, etc.) to decide.
        /// </summary>
        public virtual bool CanGenerate(ThunderKitSettings settings) => true;

        /// <summary>
        /// Generate managed stub assemblies for the configured IL2CPP game.
        /// </summary>
        /// <param name="settings">Active ThunderKit settings (game path, executable, etc.).</param>
        /// <param name="outputDirectory">An existing, empty directory to write stub .dll files into.</param>
        /// <param name="stubAssemblyPaths">Absolute paths of the produced stub assemblies.</param>
        /// <returns>True when one or more stub assemblies were produced.</returns>
        public abstract bool TryGenerate(ThunderKitSettings settings, string outputDirectory, out IReadOnlyList<string> stubAssemblyPaths);
    }
}
