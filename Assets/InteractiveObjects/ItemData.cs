using UnityEngine;
using System;

[Serializable]
public class ItemData
{
    public int id;
    public string category;
    public string type;
    public string name;
    public string description;
    
    // Опциональные поля (если нужны)
    public string iconPath;
    public string prefabPath;
}
[Serializable]
public class Category
{
    public string name;
    public string displayName;
    public string icon;
    public ItemData[] items;
}

[Serializable]
public class Database
{
    public Category[] categories;
}