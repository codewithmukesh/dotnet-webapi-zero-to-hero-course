using System.Security.Cryptography;

const string lowercase = "abcdefghijklmnopqrstuvwxyz";
const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
const string digits = "0123456789";
const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

var allChars = lowercase + uppercase + digits + special;
var length = args.Length > 0 && int.TryParse(args[0], out var len) ? len : 16;

if (length < 8 || length > 128)
{
    Console.WriteLine("Password length must be between 8 and 128 characters.");
    return;
}

var password = new char[length];
for (int i = 0; i < length; i++)
{
    password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
}

Console.WriteLine($"Generated password ({length} chars):");
Console.WriteLine(new string(password));
