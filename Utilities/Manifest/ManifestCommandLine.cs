﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

using System.ComponentModel;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Utilities.Manifest
{
    /// <summary>
    /// <see cref="CommandLine"/> for the <see cref="ManifestGenerationApplication"/> and 
    /// <see cref="ManifestInputGenerationApplication"/> applications.
    /// </summary>
    public class ManifestCommandLine : CommandLine
    {
        [CommandLineParameter("dist", "d", "Specifies the root directory of the distribution to generate a manifest for.")]
        public string DistributionDirectory { get; set; }

        [CommandLineParameter("manifest", "m", "The path to the generated manifest file.")]
        public string Manifest { get; set; }

        [CommandLineParameter("package", "p", "True if the manifest is for a package.")]
        [DefaultValue(false)]
        public bool Package { get; set; }

        [CommandLineParameter("productmanifest", "pm", "The path to the product manifest the package works against (only used when /p is specified).")]
        public string ProductManifest { get; set; }

        [CommandLineParameter("packagename", "pn", "The name of the package (only used when /p is specified).")]
        public string PackageName { get; set; }

    }
}