using MascotDesktop.Runtime.Avatar;
using NUnit.Framework;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class ModelFormatRouterTests
    {
        [Test]
        public void Classify_ReturnsImage_ForPng()
        {
            var kind = ModelFormatRouter.Classify("D:/tmp/avatar.png");
            Assert.That(kind, Is.EqualTo(ModelAssetKind.Image));
        }

        [Test]
        public void Classify_ReturnsImage_ForBmp()
        {
            var kind = ModelFormatRouter.Classify("D:/tmp/avatar.bmp");
            Assert.That(kind, Is.EqualTo(ModelAssetKind.Image));
        }

        [Test]
        public void Classify_ReturnsVrm_ForVrm()
        {
            var kind = ModelFormatRouter.Classify("D:/tmp/avatar.vrm");
            Assert.That(kind, Is.EqualTo(ModelAssetKind.Vrm));
        }

        [Test]
        public void Classify_ReturnsPmx_ForPmx()
        {
            var kind = ModelFormatRouter.Classify("D:/tmp/avatar.pmx");
            Assert.That(kind, Is.EqualTo(ModelAssetKind.Pmx));
        }

        [Test]
        public void Classify_ReturnsPmx_ForPmd()
        {
            var kind = ModelFormatRouter.Classify("D:/tmp/avatar.pmd");
            Assert.That(kind, Is.EqualTo(ModelAssetKind.Pmx));
        }

        [Test]
        public void Classify_ReturnsUnsupported_ForUnknown()
        {
            var kind = ModelFormatRouter.Classify("D:/tmp/avatar.glb");
            Assert.That(kind, Is.EqualTo(ModelAssetKind.Unsupported));
        }

        [Test]
        public void UnsupportedExtensionErrorCode_ReturnsMissing_ForNoExtension()
        {
            var code = ModelFormatRouter.UnsupportedExtensionErrorCode("D:/tmp/avatar");
            Assert.That(code, Is.EqualTo("ASSET.READ.EXTENSION_MISSING"));
        }

        [Test]
        public void UnsupportedExtensionErrorCode_ReturnsUnsupported_ForUnknownExtension()
        {
            var code = ModelFormatRouter.UnsupportedExtensionErrorCode("D:/tmp/avatar.glb");
            Assert.That(code, Is.EqualTo("ASSET.READ.UNSUPPORTED_EXTENSION"));
        }
    }
}
