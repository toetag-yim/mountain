﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Mountain.classes.dataobjects;

namespace Mountain.classes.functions {

    public static class ThreadSafeRandom {  // random function called from multiple threads (used for Shuffling an IList<T>)
        [ThreadStatic] private static Random Local;
        public static Random ThisThreadsRandom {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    public static class Extensions {


        #region generic

        #region enums

        // FruitType myBasket = FruitType.Grapes | FruitType.Oranges;

        public static bool Has<T>(this Enum enumType, T flag) { // bool hasGrapes = myBasket.Has(FruitType.Grapes); = true
            try {
                return (((int)(object)enumType & (int)(object)flag) == (int)(object)flag);
            } catch {
                return false;
            }
        }
        public static bool Is<T>(this Enum enumType, T flag) {  // bool isGrapes = myBasket.Is(FruitType.Grapes); = true
            try {
                return (int)(object)enumType == (int)(object)flag;
            } catch {
                return false;
            }
        }
        public static T Add<T>(this Enum enumType, T flag) {           // myBasket = myBasket.Add(FruitType.Apples);
            try {
                return (T)(object)(((int)(object)enumType | (int)(object)flag));
            } catch (Exception ex) {
                throw new ArgumentException("Can't add flag from enum type " + typeof(T).Name + ".", ex);
            }
        }
        public static T Remove<T>(this Enum enumType, T flag) {            // myBasket = myBasket.Remove(FruitType.Grapes);
            try {
                return (T)(object)(((int)(object)enumType & ~(int)(object)flag));
            } catch (Exception ex) {
                throw new ArgumentException("Can't remove flag from enum type " + typeof(T).Name, ex);
            }
        }
        public static bool IsEmpty<T>(this Enum flags) {
            return Convert.ToUInt32(flags) == 0;
        }

        #endregion enums

        public static T DeepClone<T>(this T input) where T : ISerializable { // true copy, so no references
            using (var stream = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, input);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
        public static void Shuffle<T>(this IList<T> list) { // randomize an interface list of <T>
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion generic

        #region strings

        public static string StripNewLine(this string str) {
            return Regex.Replace(str, @"\n|\r", "");
        }
        public static bool IsYes(this string str) { // we will just check the first char to see if the intent was Yes
            return (String.Equals(str.Substring(0, 1), "Y", StringComparison.OrdinalIgnoreCase));
        }
        public static string NewLine(this string str) {
            return str + Environment.NewLine;
        }
        public static string Indent(this string str) {
            for (int i = 0; i <= GBL.indent; i++) {
                str = " " + str;
            }
            return str;
        }
        public static string ToProper(this string str) { // make all words' first char uppercase
            TextInfo info = new CultureInfo("en-US", false).TextInfo;
            return info.ToTitleCase(str);
        }
        public static bool IsNumeric(this string str) {
            float result; // ignore output
            return float.TryParse(str, out result); // a parse error here returns false
        }
        public static bool IsValidEmailAddress(this string str) {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(str);
        }
        public static bool IsNumberOnly(this string str, bool floatPoint) { // is it a string representation of a number
            str = str.Trim();
            if (str.Length == 0) return false;
            foreach (char chr in str) {
                if (!char.IsDigit(chr)) {
                    if (floatPoint && (chr == '.')) continue;
                    return false;
                }
            }
            return true;
        }
        public static bool IsNullOrWhiteSpace(this string str) { // better string.empty test
            return string.IsNullOrWhiteSpace(str);
        }
        public static bool HasValue(this string str) {
            if (str.IsNullOrWhiteSpace()) return false;
            return true;
        }
        public static string Camelize(this string str) { // return camel-case words in string ie: "joe moe" = "JoeMoe"
            return str.ToProper().Replace(" ", string.Empty);
        }
        public static string FirstWord(this string str) { // gets the first word of a sentence
            str = str.TrimStart(' ');
            if (str.WordCount() > 1) return str.Substring(0, str.IndexOf(" "));            
            return str;
        }
        public static char FirstChar(this string str) { // get the first char of string
            return str[0];
        }
        public static bool FirstWordIsSingleChar(this string str) {  // as in is the first word i for inventory, ' for say.. 
            if (str.Length == 1) return true;
            if (str.Length > 1) if (str[1] == ' ') return true;            
            return false;
        }
        public static string StripFirstChar(this string str) {
            return str.Substring(1);
        }
        // revise this to check for a specific set of punctuations ?
        public static bool HasLastCharPunctuation(this string str) { // returns true if it ends with 'any' punctuation char, including comma..
            if (str.IsNullOrWhiteSpace()) return true; // have caller ignore empty string
            if (str.Length == 1) return Char.IsPunctuation(str[0]);
            return Char.IsPunctuation(str[str.Length - 1]);
        }
        public static string StripFirstWord(this string str) { // returns all but the first word
            if (str.WordCount() > 1) return str.Substring(str.FirstWordLength()).Trim();            
            return "";
        }
        public static int WordCount(this string str) {
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] words = str.Split(delimiterChars);
            return words.Length;
        }
        public static int FirstWordLength(this string str) {
            str = str.Trim();
            if (str.WordCount() > 1) {
                string first = str.Substring(0, str.IndexOf(" "));
                first = first.Trim();
                return first.Length;
            }
            return str.Length;
        }
        public static string StripExtraSpaces(this string str) {
            string build = str.Trim();
            string result = string.Empty;
            while (build.Length > 0) {
                result += build.FirstWord().Trim() + " ";
                build = build.StripFirstWord();
                build = build.Trim();
            }
            result = result.Trim();
            return result;
        }
        public static string WordWrap(this string longString, int width = GBL.pageWidth) {  // takes a long string and formats to width
            StringBuilder lines = new StringBuilder();
            string[] words = longString.Split(' ');
            StringBuilder buildLine = new StringBuilder("");
            foreach (var word in words) {
                if (word.Length + buildLine.Length + 1 > width) {    // check if have we exceeded line width
                    lines.Append(buildLine.ToString().Indent().NewLine());
                    buildLine.Clear();
                }
                buildLine.Append((buildLine.Length == 0 ? "" : " ") + word);  // remove space at start of new line
            }
            if (buildLine.Length > 0) lines.Append(buildLine.ToString().Indent().NewLine());   // finish up, final words to append            
            return lines.ToString();
        }

        #region cryption

        // hide & change before shifting to production
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("zk37pEji3L0t73Q5");
        private const string passPhrase = "my wee lit^le do_keydunk Duck#y DingdYnglededoo4U";

        private const int keysize = 256;
        public static string Encrypt(this string plainText) {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null)) {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged()) {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes)) {
                        using (MemoryStream memoryStream = new MemoryStream()) {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }
        public static string Decrypt(this string cipherText) {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null)) {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged()) {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes)) {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes)) {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        #endregion cryption

        #endregion strings
    }
}
