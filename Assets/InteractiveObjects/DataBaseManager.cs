using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    // Singleton для доступа из любого места
    public static ItemDatabase Instance;
    
    // Списки для хранения данных
    private Dictionary<int, ItemData> itemsById = new Dictionary<int, ItemData>();
    private Dictionary<string, List<ItemData>> itemsByCategory = new Dictionary<string, List<ItemData>>();
    
    void Awake()
    {
        // Singleton паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void LoadDatabase()
    {
        // Загружаем JSON файл из папки Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("items_database");
        
        if (jsonFile == null)
        {
            Debug.LogError("Файл items_database.json не найден в папке Resources!");
            return;
        }
        
        // Десериализуем JSON
        Database db = JsonUtility.FromJson<Database>(jsonFile.text);
        
        // Заполняем словари
        foreach (Category category in db.categories)
        {
            List<ItemData> categoryItems = new List<ItemData>();
            
            foreach (ItemData item in category.items)
            {
                // Сохраняем категорию в предмете
                item.category = category.name;
                
                // Добавляем в общий словарь
                itemsById[item.id] = item;
                
                // Добавляем в словарь категорий
                categoryItems.Add(item);
            }
            
            itemsByCategory[category.name] = categoryItems;
        }
        
        Debug.Log($"База загружена. Категорий: {itemsByCategory.Count}, Предметов: {itemsById.Count}");
    }
    
    // === ПРОСТЫЕ МЕТОДЫ ДОСТУПА ===
    
    // Получить предмет по ID
    public ItemData GetItem(int id)
    {
        if (itemsById.ContainsKey(id))
            return itemsById[id];
        return null;
    }
    
    // Получить предмет по имени
    public List<ItemData> GetCategoryItems(string categoryName)
    {
        if (itemsByCategory.ContainsKey(categoryName))
            return itemsByCategory[categoryName];
        return new List<ItemData>();
    }
    
    public List<string> GetCategoryNames()
    {
        return new List<string>(itemsByCategory.Keys);
    }
    
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(itemsById.Values);
    }
}