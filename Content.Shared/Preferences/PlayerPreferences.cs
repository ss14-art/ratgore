using System.Linq;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences
{
    /// <summary>
    ///     Contains all player characters and the index of the currently selected character.
    ///     Serialized both over the network and to disk.
    /// </summary>
    [Serializable]
    [NetSerializable]
    public sealed class PlayerPreferences
    {
        private Dictionary<int, ICharacterProfile> _characters;

        public PlayerPreferences(IEnumerable<KeyValuePair<int, ICharacterProfile>> characters, int selectedCharacterIndex, Color adminOOCColor)
        {
            _characters = new Dictionary<int, ICharacterProfile>(characters);
            AdminOOCColor = adminOOCColor;
            if (_characters.ContainsKey(selectedCharacterIndex))
            {
                SelectedCharacterIndex = selectedCharacterIndex;
            }
            else if (_characters.Count > 0)
            {
                SelectedCharacterIndex = _characters.Keys.Min();
            }
            else
            {
                SelectedCharacterIndex = 0;
            }
        }

        /// <summary>
        ///     All player characters.
        /// </summary>
        public IReadOnlyDictionary<int, ICharacterProfile> Characters => _characters;

        public ICharacterProfile GetProfile(int index)
        {
            return _characters[index];
        }

        /// <summary>
        ///     Index of the currently selected character.
        /// </summary>
        public int SelectedCharacterIndex { get; }

        /// <summary>
        ///     The currently selected character.
        /// </summary>
        public ICharacterProfile SelectedCharacter
        {
            get
            {
                if (_characters.TryGetValue(SelectedCharacterIndex, out var profile))
                {
                    return profile;
                }

                if (_characters.Count > 0)
                {
                    return _characters.Values.First();
                }

                return HumanoidCharacterProfile.Random();
            }
        }

        public Color AdminOOCColor { get; set; }

        public int IndexOfCharacter(ICharacterProfile profile)
        {
            return _characters.FirstOrNull(p => p.Value == profile)?.Key ?? -1;
        }

        public bool TryIndexOfCharacter(ICharacterProfile profile, out int index)
        {
            return (index = IndexOfCharacter(profile)) != -1;
        }
    }
}
