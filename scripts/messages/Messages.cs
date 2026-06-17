using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

public interface MessageInterface
{
    GDictionary to_payload();
    void from_payload(GDictionary payload);
    string[] validate();
}

public interface CommandInterface { }

public interface QueueableCommandInterface { }

public abstract partial class MessageBase : RefCounted, MessageInterface
{
    protected static GDictionary D() => new();
    protected static GDictionary D(params (string Key, Variant Value)[] items)
    {
        var d = new GDictionary();
        foreach (var item in items) d[item.Key] = item.Value;
        return d;
    }
    public static bool Has(GDictionary d, string key) => d.ContainsKey(key);
    protected static string S(GDictionary d, string key, string fallback = "") => Has(d, key) ? d[key].AsString() : fallback;
    protected static StringName SN(GDictionary d, string key, StringName fallback = default) => Has(d, key) ? new StringName(d[key].AsString()) : fallback;
    protected static int I(GDictionary d, string key, int fallback = 0) => Has(d, key) ? d[key].AsInt32() : fallback;
    protected static float F(GDictionary d, string key, float fallback = 0.0f) => Has(d, key) ? (float)d[key].AsDouble() : fallback;
    protected static bool B(GDictionary d, string key, bool fallback = false) => Has(d, key) ? d[key].AsBool() : fallback;
    protected static Vector2 V2(GDictionary d, string key, Vector2 fallback = default) => Has(d, key) ? d[key].AsVector2() : fallback;
    protected static Variant Var(GDictionary d, string key) => Has(d, key) ? d[key] : default;
    protected static string[] PSA(GDictionary d, string key) => Has(d, key) ? d[key].AsStringArray() : System.Array.Empty<string>();
    protected static byte[] PBA(GDictionary d, string key) => Has(d, key) ? d[key].AsByteArray() : System.Array.Empty<byte>();
    protected static GDictionary GD(GDictionary d, string key) => Has(d, key) ? d[key].AsGodotDictionary() : new GDictionary();
    protected static GArrayDictionary GAD(GDictionary d, string key) => Has(d, key) ? d[key].AsGodotArray<GDictionary>() : new GArrayDictionary();
    protected static bool Empty(string value) => string.IsNullOrEmpty(value);
    protected static bool Empty(StringName value) => value == default || value == new StringName("");
    protected static bool Empty(string[] value) => value == null || value.Length == 0;
    protected static bool Empty(byte[] value) => value == null || value.Length == 0;
    protected static bool Empty(GDictionary value) => value.Count == 0;
    protected static bool Empty(GArrayDictionary value) => value.Count == 0;
    protected static bool QueueModeValid(int value) => value == (int)GameMessages.QueueMode.REPLACE || value == (int)GameMessages.QueueMode.APPEND;
    protected static bool VariantIsNil(Variant value) => value.VariantType == Variant.Type.Nil;
    protected static bool VariantIsVector2(Variant value) => value.VariantType == Variant.Type.Vector2;
    protected static bool VariantIsDictionary(Variant value) => value.VariantType == Variant.Type.Dictionary;
    protected static bool VariantIsBool(Variant value) => value.VariantType == Variant.Type.Bool;
    protected static bool VariantIsColor(Variant value) => value.VariantType == Variant.Type.Color;
    protected static string VariantString(Variant value) => value.AsString();
    protected static bool DictStringEmpty(GDictionary d, string key) => !Has(d, key) || string.IsNullOrEmpty(d[key].AsString());
    protected static bool DictIntInvalid(GDictionary d, string key) => !Has(d, key) || d[key].AsInt32() <= 0;
    protected static bool DictIntNegative(GDictionary d, string key) => !Has(d, key) || d[key].AsInt32() < 0;
    protected static void Add(List<string> errors, string error) => errors.Add(error);
    public abstract GDictionary to_payload();
    public abstract void from_payload(GDictionary payload);
    public virtual string[] validate() => System.Array.Empty<string>();
}

public abstract partial class UnitDestinationCommandBase : MessageBase, CommandInterface, QueueableCommandInterface
{
    public string player_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] unit_ids = System.Array.Empty<string>();
    public Vector2 destination = Vector2.Zero;
    public StringName formation = "rectangle";
    protected GDictionary UnitDestinationPayload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("unit_ids", unit_ids), ("destination", destination), ("formation", formation));
    protected void UnitDestinationFromPayload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        unit_ids = PSA(payload, "unit_ids");
        destination = V2(payload, "destination");
        formation = SN(payload, "formation", "rectangle");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(unit_ids)) Add(errors, "unit_ids must contain at least one unit.");
        if (formation != new StringName("none") && formation != new StringName("rectangle")) Add(errors, "formation is invalid.");
        return errors.ToArray();
    }
}
