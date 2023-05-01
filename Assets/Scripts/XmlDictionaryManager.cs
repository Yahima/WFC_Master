using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class RulesData
{
    public List<Rule> rules;

    public RulesData()
    {
        rules = new List<Rule>();
    }
}

public class Rule
{
    public string hash;
    public List<DirectionData> directions;

    public Rule()
    {
        directions = new List<DirectionData>();
    }

    public Rule(string hash)
    {
        this.hash = hash;
        directions = new List<DirectionData>();
    }
}

public class DirectionData
{
    public Direction direction;

    public List<string> valids;

    public DirectionData()
    {
        valids = new List<string>();
    }

    public DirectionData(Direction direction)
    {
        this.direction = direction;
        valids = new List<string>();
    }
}

public class XmlDictionaryManager
{
    private readonly string _filePath;

    public XmlDictionaryManager(string filePath)
    {
        _filePath = filePath;
    }

    public void Save(Dictionary<string, Dictionary<Direction, List<string>>> dictionary)
    {
        RulesData data = new();

        foreach (KeyValuePair<string, Dictionary<Direction, List<string>>> rules in dictionary)
        {
            Rule rule = new(rules.Key);
            foreach (KeyValuePair<Direction, List<string>> directionData in rules.Value)
            {
                DirectionData direction = new(directionData.Key);
                direction.valids.AddRange(directionData.Value);
                rule.directions.Add(direction);
            }

            data.rules.Add(rule);
        }

        XmlSerializer serializer = new(typeof(RulesData));
        using StreamWriter writer = new(_filePath);
        serializer.Serialize(writer, data);
    }

    public Dictionary<string, Dictionary<Direction, List<string>>> Load()
    {
        Dictionary<string, Dictionary<Direction, List<string>>> dictionary = new();

        XmlSerializer serializer = new(typeof(RulesData));
        using (StreamReader reader = new(_filePath))
        {
            RulesData data = (RulesData)serializer.Deserialize(reader);

            foreach (Rule rule in data.rules)
            {
                Dictionary<Direction, List<string>> directions = new();
                foreach (DirectionData directionData in rule.directions)
                {
                    directions[directionData.direction] = directionData.valids;
                }
                dictionary[rule.hash] = directions;
            }
        }

        return dictionary;
    }
}