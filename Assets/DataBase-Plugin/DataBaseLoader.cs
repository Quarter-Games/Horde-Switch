using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class DataBaseLoader
{    
    public static async Task<Dictionary<TKey, TValue>> ReadTableAsync<TKey, TValue>(string tableName, Func<string, TKey> keyParser, Func<List<string>, TValue> valueConstructor,SheetsSettings API_settings)
    {
        Dictionary<TKey, TValue> values = new();
        DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
        UnityWebRequest Sheets = new UnityWebRequest($"https://sheets.googleapis.com/v4/spreadsheets/{API_settings.SHEET_ID}/values/{tableName}/?alt=json&key={API_settings.KEY}", "GET", handler, null);
        await Sheets.SendWebRequest();
        if (Sheets.result == UnityWebRequest.Result.ConnectionError || Sheets.result == UnityWebRequest.Result.DataProcessingError
            || Sheets.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error" + Sheets.error);
            throw new Exception(Sheets.error);
        }
        else
        {
            var data = JsonConvert.DeserializeObject<SpreadSheetData>(Sheets.downloadHandler.text);
            for (int i = 1; i < data.values.Count; i++)
            {
                TKey name = keyParser(data.values[i][0]);
                TValue value = valueConstructor(data.values[i]);
                values.Add(name, value);
            }
        }
        return values;
    }
    [Serializable]
    public struct SpreadSheetData
    {
        [SerializeField] public string range;
        [SerializeField] public string majorDimension;
        [SerializeField] public List<List<string>> values;
    }
}
