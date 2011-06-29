﻿using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Imaging;
using ClearCanvas.ImageViewer.StudyManagement;

namespace ClearCanvas.ImageViewer.Tools.Standard.PresetVoiLuts
{
    internal static class LutHelper
    {
        public static bool IsModalityLutProvider(IPresentationImage presentationImage)
        {
            return presentationImage is IModalityLutProvider;
        }

        public static bool IsVoiLutProvider(IPresentationImage presentationImage)
        {
            return presentationImage is IVoiLutProvider;
        }

        public static bool IsVoiLutEnabled(IPresentationImage presentationImage)
        {
            var provider = presentationImage as IVoiLutProvider;
            return provider != null && provider.VoiLutManager.Enabled;
        }

        public static bool IsImageSopProvider(IPresentationImage presentationImage)
        {
            return presentationImage is IImageSopProvider;
        }

        public static bool IsDicomVoiLutsProvider(IPresentationImage presentationImage)
        {
            return presentationImage is IDicomVoiLutsProvider;
        }

        public static bool IsGrayScaleImage(IPresentationImage presentationImage)
        {
            var graphicProvider = presentationImage as IImageGraphicProvider;
            return graphicProvider != null && graphicProvider.ImageGraphic.PixelData is GrayscalePixelData;
        }

        public static bool IsColorImage(IPresentationImage presentationImage)
        {
            var graphicProvider = presentationImage as IImageGraphicProvider;
            return graphicProvider != null && graphicProvider.ImageGraphic.PixelData is ColorPixelData;
        }
    }
}
