using System;
using System.Collections.Generic;

public static class FileExtensionExtractor
{
    // Define a dictionary mapping file signatures to file extensions
    private static readonly Dictionary<string, string> FileSignatures = new Dictionary<string, string>()
    {
        { "25504446", ".pdf" },     // PDF
        { "FFD8FFDB", ".jpg" },     // JPEG
        { "FFD8FFE0", ".jpg" },     // JPEG
        { "FFD8FFE1", ".jpg" },     // JPEG
        { "47494638", ".gif" },     // GIF
        { "474946383761", ".gif" }, // GIF
        { "474946383961", ".gif" }, // GIF
        { "89504E47", ".png" },     // PNG
        { "504B0304", ".zip" },     // ZIP
        { "504B0506", ".zip" },     // ZIP
        { "504B0708", ".zip" }      // ZIP
        // Add more as needed
    };

    public static string GetExtension(byte[] bytes)
    {
        string signature = GetSignature(bytes);
        if (FileSignatures.TryGetValue(signature, out string extension))
        {
            return extension;
        }
        return ".bin"; // Default extension if signature not found
    }

    private static string GetSignature(byte[] bytes)
    {
        // Convert first few bytes to hex string
        string hexSignature = BitConverter.ToString(bytes.Take(4).ToArray()).Replace("-", "");
        return hexSignature;
    }
}

