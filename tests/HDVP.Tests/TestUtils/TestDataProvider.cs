using System.Text;

using JetBrains.Annotations;

namespace HDVP.TestUtils
{
    internal static class TestDataProvider
    {
        [MustUseReturnValue]
        public static HdvpVerifiableData CreateVerifiableData()
        {
            var dataBytes = Encoding.UTF8.GetBytes(
                "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. " +
                "At vero eos et accusam et justo duo dolores et ea rebum."
            );

            return new HdvpVerifiableData(dataBytes);
        }

        [MustUseReturnValue]
        public static HdvpSalt CreateNonRandomSalt()
        {
            return new(CreateNonRandomSaltRaw());
        }

        [MustUseReturnValue]
        public static byte[] CreateNonRandomSaltRaw()
        {
            // NOTE: Must be exactly 32 bytes.
            return Encoding.ASCII.GetBytes("Stet clita kasd gubergren, no se");
        }
    }
}
