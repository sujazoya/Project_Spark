using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ProjectSpark.Gameplay.SecureEnclave
{
    internal static class SparkLevel115Firmware
    {
        internal sealed class Image
        {
            public readonly string Payload, Digest, Signature, Signer;
            public readonly int Version;
            internal Image(int version, string signer, string payload, string digest, string signature)
                => (Version, Signer, Payload, Digest, Signature) = (version, signer, payload, digest, signature);
        }

        internal const int PolicyRevision = 1;
        internal const int MinVersion = 2;
        internal static readonly string[] Profiles = { "Workshop signed v3", "Corrupted read", "Unlisted signer", "Signed legacy v1" };
        private static readonly Image Approved = new(3, "Workshop Root", "SHX-23 verified boot demonstration image revision 3",
            "uNoAs4ES66DoW1LO0XK9pp8suCvoY+9C/1tc87EXUVg=",
            "KSoGZs1hNdZ5tF6XFKyt1iBDqh6zP2KpxgYUzOmd+eousejyVi4CSHXIxpXPxieC4yh98DLY6kfFrC9JNysXkMEyXpNjQ1ShqtMkIIeMLXH1oguuRKzPHMQIaWmqAr8+W3IMs4Z8A/Uic7uDpHj2UDb/Nky8rj7apIfKvb9njIshOWLhfpeuYom9+BguRG3AC8T351PSK47uxOcE2CzGSWcEH2zRuI7g3PAQxf2JDnutmReopUbseQbb6ZaRHjLV8X+7KN6zwEhs3ih+yGGFK1v1QG2tQlyWnN8nlc5cRvoxMrnf2R9B/JDSEVVXBn0/85AR5AN1YQeuvgU0nyZyew==");
        private static readonly Image Legacy = new(1, "Workshop Root", "SHX-23 verified boot demonstration image revision 1",
            "h4oPBAFMjX+l/UoFaoSvYwZ79AWZWBe1raiLF6cgPrM=",
            "M7/CbJQNjbRbBAHMugG/t3F5aTR5ZP72KeEosrMUheJaW2pag4O3yh5Mari401s+LjhNArcVnUOLOuA1L10OTGZ8/kmUCQfalP2dkHfUQqRElAP+vJHxp7S23t2wk1Rtu7z/Nty5o4GIKLgd3ZkoLClAi3zjV1nsSOhPi9Q6MPDBrqWjLC2ELeOLAQfFxk1n5cygD3tN1Yitu38H7E66i5BP6BZJLAOtbsLlXxhYM/Ww+grKkjEBmz+zBqtFmHC1XSzchbE0F+1a6SbPiZ6B+hpxk5B712OXlCLzglPu70NuqUniMmi0VH/owE9FVRS9WslvCS0WPCxPq7obcVlvmw==");
        private static readonly Image Unlisted = new(3, "Unlisted Workshop", "SHX-23 signature rejection training image",
            "2FkinmL+YusUeecng1YZHOyGLBSjQivvNtnPSl3Mo5o=",
            "ix34PsVFnK+cR8HKBrrdzEb16x9FJ2uca+5t4zpYf/OOJ4lqRLzjru4EWB+zF50qYKNM3TaTbkftXswwIhL7fuV0yYrBXBY2Tgt59sOm4M2/bYkh5tSce3IzOZ+3g0IQdSMBiJ7L8dmLxZ3OXaAstMnLHr/07KChtvWGkP3F0BJM7oejepSnBePok1JkbVHkqfnWa5MHMD48BpZdJfe3oRUNGOlaD7+KmGhfqdn+oCvseiNJQjZvTUYnaHiTzyIEA/31DWeH24gvWehvLnjSNbJS8p/5RAwpIY4WfvIek6ODeY2QuWIX7qqT/kD3KeGPnUGyuz85vWZKwejQbNJPWA==");
        private const string RootModulus = "v/0//tt/rKG1dgV432BdkAVaoS/mepKzERX1MBgktzpqeElVTREZ8t9uuVE7gAE0hMyt06sAjmvnFVeppNH+Vp0C4X1zmAwAB4MF5wi7eyWeE03cDLBQhB4WkHG4Wbtntf/0cTYS8O+yDYpQ4t/1+GwTgEZQwacQ38qo2cbUVBJTu0y0PU4ElgFvAje897rmy9CItHq6cMwm9YaedfKsCfh1hNvUysMfEUZZaiXSAQfixqiMB1oNfKMW7D2ktdMu7iA1DwrW97CYAfaVIcqnR8TiRN55WEinMfbbK0j7PeUskSVzd6fxHrW5gwB8IbSzgNqufUYwVT13zBpc7LN0+Q==";
        private const string RootExponent = "AQAB";

        internal static Image Select(int profile) => profile switch
        {
            1 => new Image(Approved.Version, Approved.Signer, Approved.Payload + " [different bytes]",
                Approved.Digest, Approved.Signature),
            2 => Unlisted, 3 => Legacy, _ => Approved
        };

        internal static string Digest(string payload)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        }

        internal static bool VerifySignature(Image image)
        {
            if (image.Signer != Approved.Signer) return false;
            using var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Convert.FromBase64String(RootModulus), Exponent = Convert.FromBase64String(RootExponent)
            });
            string manifest = "SHX23|" + image.Version.ToString(CultureInfo.InvariantCulture) + "|" +
                image.Signer + "|" + image.Digest;
            return rsa.VerifyData(Encoding.UTF8.GetBytes(manifest), Convert.FromBase64String(image.Signature),
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    internal sealed class SparkLevel115Attestor : IDisposable
    {
        private readonly RSA device;
        private readonly RSA verifier;
        private byte[] signature;
        private string nonce;

        internal SparkLevel115Attestor()
        {
            device = RSA.Create();
            device.KeySize = 2048;
            verifier = RSA.Create();
            verifier.ImportParameters(device.ExportParameters(false));
        }

        internal string Challenge(string snapshot)
        {
            var bytes = new byte[24];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            nonce = Convert.ToBase64String(bytes);
            signature = device.SignData(Message(snapshot), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return nonce;
        }

        internal bool Verify(string snapshot)
        {
            if (signature == null) return false;
            byte[] response = signature;
            signature = null;
            return verifier.VerifyData(Message(snapshot), response, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        private byte[] Message(string snapshot) => Encoding.UTF8.GetBytes("SHX23-QUOTE|" + nonce + "|" + snapshot);
        internal void Clear() { signature = null; nonce = null; }
        public void Dispose() { device.Dispose(); verifier.Dispose(); }
    }
}
