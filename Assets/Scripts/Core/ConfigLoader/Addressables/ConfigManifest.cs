using System.Collections.Generic;

[System.Serializable]
public class ConfigManifest
{
    public List<Entry> Items = new();

    [System.Serializable]
    public class Entry
    {
        public string Name;
        public string Version;
    }

    public bool TryGet(string name, out string version)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Name == name) { version = Items[i].Version; return true; }
        }
        version = null;
        return false;
    }
}