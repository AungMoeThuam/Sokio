
namespace Sokio
{
    public interface IPersistence
    {
        public Tuple<string, byte[]> readBinaryFile(string fileName);
        public void writeBinaryFile(string fileName, byte[] binaryFile);
    }
}