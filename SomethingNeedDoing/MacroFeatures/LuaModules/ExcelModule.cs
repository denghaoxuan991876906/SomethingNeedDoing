using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Reflection;
using Lumina.Text.ReadOnly;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;

public class ExcelModule : LuaModuleBase
{
    public override string ModuleName => "Excel";

    private const BindingFlags PropertyFlags = BindingFlags.Public | BindingFlags.Instance |
                                               BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly;

    public SheetWrapper? this[string name] => GetSheet(name);

    public SheetWrapper? GetSheet(string name)
    {
        var rawType = typeof(Addon).Assembly.GetType($"Lumina.Excel.Sheets.{name}", false, true);
        if (rawType == null) return null;

        MethodInfo? method;
        var isSubRow = false;
        if (rawType.IsAssignableTo(typeof(IExcelSubrow<>).MakeGenericType(rawType)))
        {
            method = typeof(IDataManager).GetMethod(nameof(IDataManager.GetSubrowExcelSheet))?.MakeGenericMethod(rawType);
            isSubRow = true;
        }
        else
        {
            method = typeof(IDataManager).GetMethod(nameof(IDataManager.GetExcelSheet))?.MakeGenericMethod(rawType);
        }

        var sheet = method?.Invoke(Svc.Data, [null, null]);
        return sheet == null ? null : new SheetWrapper(sheet, isSubRow);
    }

    private static object? GetPropertyValue(object? obj, string propertyName)
    {
        var property = obj?.GetType().GetProperty(propertyName, PropertyFlags);
        if (property == null) return null;
        var rawValue = property.GetValue(obj);
        if (rawValue == null) return null;

        var type = rawValue.GetType();
        if (type.IsAssignableTo(typeof(RowRef)))
            return GetRowIdFromObject(rawValue);

        if (rawValue is ReadOnlySeString seString)
            return seString.ExtractText();

        if (IsRowRef(type))
            return GetRowRefValue(rawValue);

        if (IsCollection(type))
            return new CollectionWrapper(rawValue);

        return rawValue;
    }

    private static uint? GetRowIdFromObject(object obj)
    {
        var prop = obj.GetType().GetProperty("RowId");
        var value = prop?.GetValue(obj);
        return value is uint rowId ? rowId : null;
    }

    private static RowWrapper? GetRowRefValue(object rowRef)
    {
        var isValid = rowRef.GetType().GetProperty(nameof(RowRef<>.IsValid));
        if (isValid != null && isValid.GetValue(rowRef) is not true)
            return null;

        var property = rowRef.GetType().GetProperty(nameof(RowRef<>.Value));
        var value = property?.GetValue(rowRef);
        return value == null ? null : new RowWrapper(value);
    }

    private static bool IsCollection(Type? type)
    {
        var genericArg = type?.GenericTypeArguments.FirstOrDefault();
        if (genericArg == null) return false;
        return type?.IsAssignableTo(typeof(Collection<>).MakeGenericType(genericArg)) == true;
    }

    private static bool IsRowRef(Type? type)
    {
        var sheetType = GetGenericSheetType(type);
        return sheetType != null && type?.IsAssignableTo(typeof(RowRef<>).MakeGenericType(sheetType)) == true;
    }

    private static Type? GetGenericSheetType(Type? type)
    {
        var arg = type?.GenericTypeArguments.FirstOrDefault();
        return arg?.GetCustomAttribute<SheetAttribute>() != null ? arg : null;
    }

    public class SheetWrapper(object sheet, bool isSubrowSheet)
    {
        public object? this[int rowId] => GetRow(rowId);

        public object? GetRow(int rowId)
        {
            if (!isSubrowSheet)
            {
                var method = sheet.GetType().GetMethod(nameof(ExcelSheet<>.GetRowOrDefault));
                var row = method?.Invoke(sheet, [(uint)rowId]);

                // return the string directly if this is an addon
                if (row is Addon addonRow)
                    return addonRow.Text.ExtractText();

                // return the value directly if there is only 1 property
                // uncomment for more direct access
                // e.g "excel.actionprocstatus[1].name" instead of "excel.actionprocstatus[1].status.name"
                //if (GetSinglePropertyValue(row) is { } value)
                //    return value;

                return row == null ? null : new RowWrapper(row);
            }

            var hasRow = sheet.GetType().GetMethod(nameof(SubrowExcelSheet<>.HasRow));
            if (hasRow == null || hasRow.Invoke(sheet, [(uint)rowId]) is not true)
                return null;
            return new SubRowWrapper(sheet, (uint)rowId);
        }

        private static object? GetSinglePropertyValue(object? row)
        {
            var props = row?.GetType().GetProperties(PropertyFlags);
            if (props is not { Length: 2 })
                return null;
            var prop = props[0].Name.Equals("RowId") ? props[1] : props[0];
            return GetPropertyValue(row, prop.Name);
        }
    }

    public class RowWrapper(object row)
    {
        public object? this[string propertyName] => GetPropertyValue(row, propertyName);
    }

    public class SubRowWrapper(object sheet, uint rowId)
    {
        public RowWrapper? this[int subRowId] => GetSubRow(subRowId);

        public RowWrapper? GetSubRow(int subRowId)
        {
            var method = sheet.GetType().GetMethod(nameof(SubrowExcelSheet<>.GetSubrowOrDefault));
            var row = method?.Invoke(sheet, [rowId, (ushort)subRowId]);
            return row == null ? null : new RowWrapper(row);
        }
    }

    public class CollectionWrapper(object collection)
    {
        public object? this[int index] => GetValue(index);

        private object? GetValue(int index)
        {
            if (!CheckBounds(collection, index))
                return null;

            var indexer = GetIndexer(collection.GetType());
            var value = indexer?.GetValue(collection, [index]);
            if (value == null) return null;

            if (value is ReadOnlySeString seString)
                return seString.ExtractText();

            if (value.GetType().IsAssignableTo(typeof(RowRef)))
                return GetRowIdFromObject(value);

            if (IsRowRef(value.GetType()))
                return GetRowRefValue(value);

            return value;
        }

        private static bool CheckBounds(object collection, int index)
        {
            var prop = collection.GetType().GetProperty(nameof(Collection<>.Count), PropertyFlags);
            if (prop == null) return false;
            if (prop.GetValue(collection) is not int count)
                return false;
            return index >= 0 && index < count;
        }

        private static PropertyInfo? GetIndexer(Type type)
        {
            return type.GetProperty("Item", PropertyFlags);
        }
    }
}
