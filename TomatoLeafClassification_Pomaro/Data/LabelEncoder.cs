using System;
using System.Collections.Generic;

namespace TomatoLeafClassification.Data
{
    /// <summary>
    /// Maps categorical class names (the dataset sub-folder names) to integer ids and back. Ids are
    /// assigned in alphabetical order so that the mapping is deterministic and reproducible.
    /// </summary>
    public class LabelEncoder
    {
        private Dictionary<string, int> _nameToId;
        private List<string> _idToName;

        /// <summary>
        /// Creates an empty encoder; call <see cref="Fit"/> to learn the classes.
        /// </summary>
        public LabelEncoder()
        {
            _nameToId = new Dictionary<string, int>();
            _idToName = new List<string>();
        }

        /// <summary>
        /// Number of distinct classes learned by the encoder.
        /// </summary>
        public int Count
        {
            get { return _idToName.Count; }
        }

        /// <summary>
        /// Learns the label set from a collection of class names.
        /// </summary>
        /// <exception cref="ArgumentException">If the collection is null or empty.</exception>
        public void Fit(List<string> classNames)
        {
            if (classNames == null || classNames.Count == 0)
                throw new ArgumentException("At least one class name is required.", "classNames");

            List<string> distinct = new List<string>();
            foreach (string name in classNames)
            {
                if (!distinct.Contains(name))
                    distinct.Add(name);
            }

            distinct.Sort(StringComparer.Ordinal);

            foreach (string name in distinct)
            {
                _nameToId[name] = _idToName.Count;
                _idToName.Add(name);
            }
        }

        /// <summary>
        /// Class name to integer id.
        /// </summary>
        /// <exception cref="KeyNotFoundException">If the name was not seen during <see cref="Fit"/>.</exception>
        public int Encode(string className)
        {
            int id;
            if (!_nameToId.TryGetValue(className, out id))
                throw new KeyNotFoundException(
                    "Unknown class '" + className + "'. Did you call Fit first?");
            return id;
        }

        /// <summary>
        /// Integer id to class name.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the id is outside the learned range.
        /// </exception>
        public string Decode(int id)
        {
            if (id < 0 || id >= _idToName.Count)
                throw new ArgumentOutOfRangeException("id", "Unknown class id: " + id + ".");
            return _idToName[id];
        }

        /// <summary>
        /// Override of ToString() to show the number of classes and a few of their names.
        /// </summary>
        public override string ToString()
        {
            string result = "LabelEncoder: " + _idToName.Count + " classes";
            if (_idToName.Count == 0) return result;

            result = result + " [";
            int shown = _idToName.Count;
            if (shown > 3) shown = 3;
            for (int i = 0; i < shown; i++)
            {
                if (i > 0) result = result + ", ";
                result = result + _idToName[i];
            }
            if (_idToName.Count > shown) result = result + ", ...";
            return result + "]";
        }
    }
}
