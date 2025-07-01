
namespace Sokio
{
    public class BinaryFileStore : IPersistence
    {
        private readonly string _folder;

        public BinaryFileStore(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                throw new ArgumentException("File path must be provided");

            if (!Directory.Exists(folderName))
                throw new DirectoryNotFoundException($"The folder '{folderName}' does not exist.");
            _folder = folderName;
        }

        public Tuple<string, byte[]> readBinaryFile(string fileName)
        {
            string filePath = Path.Combine(_folder, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return new Tuple<string, byte[]>(fileName, File.ReadAllBytes(filePath));
        }

        public void writeBinaryFile(string fileName, byte[] binaryFile)
        {
            if (binaryFile == null)
                throw new ArgumentNullException(nameof(binaryFile), "Binary data must not be null");

            string filePath = Path.Combine(_folder, fileName);

            // Ensure the directory exists
            Directory.CreateDirectory(_folder);

            File.WriteAllBytes(filePath, binaryFile);
        }
    }
}