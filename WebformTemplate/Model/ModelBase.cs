using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System;
using System.Linq;
using FastMember;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Collections;

namespace WebformTemplate.Model
{
    public class ModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(caller)); }
        }

        public static string Server
        {
            get
            {
                string str = ConfigurationManager.AppSettings["SQLServer"];
                if (string.IsNullOrEmpty(str))
                {
                    return "";
                    //throw new Exception("SQLSERVERELEMENTMISSING");
                }
                else
                {
                    return str;
                }
            }
        }

        public static string Database
        {
            get
            {
                string str = ConfigurationManager.AppSettings["Database"];
                if (string.IsNullOrEmpty(str))
                {
                    //throw new Exception("DATABASEELEMENTMISSING");
                    return "";
                }
                else
                {
                    return str;
                }
            }
        }
        public static string sqlString = "Data Source=" + Server + ";Initial Catalog=" + Database + ";Integrated Security=SSPI;";

        #region Validation
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        internal Dictionary<string, List<string>> Errors => _errors;

        public string Error
        {
            get
            {
                var builder = new System.Text.StringBuilder();
                foreach (var error in this.Errors)
                {
                    if (error.Value.Count > 0)
                    {
                        foreach (var text in error.Value)
                        {
                            builder.AppendLine(text);
                        }
                    }
                }
                return builder.Length > 0 ? builder.ToString(0, builder.Length - 2) : builder.ToString();
            }
        }

        public bool HasError => Errors.Count > 0;

        public string this[string columnName]
        {
            get
            {
                var modelClassProperties = TypeDescriptor.GetProperties(this.GetType());
                foreach (PropertyDescriptor prop in modelClassProperties)
                {
                    if (prop.Name != columnName)
                        continue;

                    Errors[columnName] = new List<string>();
                    foreach (var attribute in prop.Attributes)
                    {
                        if (!(attribute is System.ComponentModel.DataAnnotations.ValidationAttribute))
                            continue;

                        var validation = attribute as System.ComponentModel.DataAnnotations.ValidationAttribute;

                        if (validation.IsValid(prop.GetValue(this)))
                            continue;

                        var dn = prop.Name;
                        foreach (var pa in prop.Attributes.OfType<DisplayNameAttribute>())
                        {
                            dn = pa.DisplayName;
                        }
                        Errors[columnName].Add(validation.FormatErrorMessage(dn));
                        RaisePropertyChanged("Error");
                        return validation.FormatErrorMessage(dn);
                    }
                }
                Errors.Remove(columnName);
                RaisePropertyChanged("Error");
                return null;
            }
        }

        #endregion
    }
    public static class ModelBaseExtras
    {
        #region SafeGetters
        public static T GetFieldValue<T>(this SqlDataReader reader, string fieldName, T defaultValue = default(T))
        {
            try
            {
                var value = reader[fieldName];
                if (value == System.DBNull.Value || value == null)
                    return defaultValue;
                return (T)value;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public static int? SafeGetNullableInt(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<int?>(reader, name, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool SafeGetBoolean(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<bool>(reader, name, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Guid SafeGetGuid(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<Guid>(reader, name, Guid.Empty);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool? SafeGetNullableBoolean(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<bool?>(reader, name, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static double SafeGetDouble(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<double>(reader, name, 0.0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static float SafeGetFloat(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<float>(reader, name, 0.0f);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static System.DateTime? SafeGetNullableDateTime(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<DateTime?>(reader, name, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static System.DateTime SafeGetDateTime(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<DateTime>(reader, name, DateTime.MinValue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string SafeGetString(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<String>(reader, name, (string)null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static int SafeGetInt32(this SqlDataReader reader, string name)
        {
            try
            {
                return GetFieldValue<int>(reader, name, -1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public static T ConvertToObject<T>(this SqlDataReader reader) where T : class, new()
        {
            Type type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();
            var t = new T();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (!reader.IsDBNull(i))
                {
                    string fieldName = reader.GetName(i);

                    if (members.Any(m => string.Equals(m.Name, fieldName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var test = reader.GetValue(i);
                        accessor[t, fieldName] = reader.GetValue(i);
                    }
                }
            }

            if (members.Any(x => x.Type.Equals(type)))
            {
                var member = members.First(x => x.Type.Equals(type));
                accessor[t, member.Name] = t.Copy();
            }

            return t;
        }

        public static void Store<T>(this T obj, List<SqlParameter> sqlParams, string storedProc)
        {
            using (SqlConnection con = new SqlConnection(ModelBase.sqlString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProc, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (sqlParams != null)
                    {
                        foreach (SqlParameter t in sqlParams)
                            cmd.Parameters.Add(t);
                    }
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public static int Store<T>(this T obj, SqlTransaction transaction = null, SqlConnection con = null, string altSqlString = null)
        {
            try
            {
                bool singleTransaction = false;
                int num = -1;
                if (con == null)
                {
                    singleTransaction = true;
                    if (!String.IsNullOrWhiteSpace(altSqlString))
                        con = new SqlConnection(altSqlString);
                    else
                        con = new SqlConnection(ModelBase.sqlString);
                    con.Open();
                    transaction = con.BeginTransaction();
                }

                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Connection = con;
                string str = obj.StoredProcedureSave();
                if (string.IsNullOrEmpty(str)) throw new Exception("No Save Stored Procedure Defined");
                cmd.CommandText = str;
                cmd.Transaction = transaction;

                List<SqlParameter> sqlParams = obj.ToSqlParamsList();

                foreach (SqlParameter sqlParam in sqlParams)
                {
                    cmd.Parameters.Add(sqlParam);
                }
                object id = cmd.ExecuteScalar();
                if (id != null && id != DBNull.Value)
                    num = Convert.ToInt32(id);
                else
                    num = -1;

                if (singleTransaction)
                {
                    transaction.Commit();
                    con.Close();
                }

                return num;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool Delete<T>(this T obj, SqlTransaction transaction = null, SqlConnection con = null)
        {
            try
            {
                bool singleTransaction = false;
                bool b = false;
                if (con == null)
                {
                    singleTransaction = true;
                    con = new SqlConnection(ModelBase.sqlString);
                    con.Open();
                    transaction = con.BeginTransaction();
                }

                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Connection = con;
                string str = obj.StoredProcedureDelete();
                if (string.IsNullOrEmpty(str)) throw new Exception("No Save Stored Procedure Defined");
                cmd.CommandText = str;
                cmd.Transaction = transaction;

                List<SqlParameter> sqlParams = obj.ToSqlParamsList();

                SqlParameter sqlParam = obj.ToSqlPrimaryIDParam();

                cmd.Parameters.Add(sqlParam);

                cmd.ExecuteNonQuery();

                if (singleTransaction)
                {
                    transaction.Commit();
                    con.Close();
                }

                b = true;
                return b;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<ObservableCollection<T>> CollectData<T>(this string proc, SQLParam[] sqlParams = null, string conString = null) where T : class, new()
        {
            try
            {
                ObservableCollection<T> col = new ObservableCollection<T>();

                if (string.IsNullOrWhiteSpace(conString))
                    conString = ModelBase.sqlString;

                using (SqlConnection con = new SqlConnection(conString))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.CommandText = proc;
                    if (sqlParams != null)
                    {
                        foreach (SQLParam t in sqlParams)
                        {
                            if (t.Value != null)
                                cmd.Parameters.Add(t.VariableName, t.SqlType).Value = Convert.ChangeType(t.Value, t.DataType);
                        }
                    }

                    con.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            col.Add(reader.ConvertToObject<T>());
                        }
                    }
                    con.Close();
                }
                return col;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public static async Task<T> CollectData<T>(this string proc, SQLParam[] sqlParams, string str, string param = null) where T : class, new()
        {
            try
            {
                T obj = new T();

                if (string.IsNullOrWhiteSpace(str))
                    str = ModelBase.sqlString;


                using (SqlConnection con = new SqlConnection(str))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.CommandText = proc;
                    if (sqlParams != null)
                    {
                        foreach (SQLParam t in sqlParams)
                        {
                            if (t.Value != null)
                                cmd.Parameters.Add(t.VariableName, t.SqlType).Value = Convert.ChangeType(t.Value, t.DataType);
                        }
                    }

                    con.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            obj = reader.ConvertToObject<T>();
                        }
                    }
                    con.Close();
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public static bool Compare<T>(this T A)
        {
            var type = typeof(T);
            var compareProp = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly).FirstOrDefault(x => Attribute.IsDefined(x, typeof(OriginalCopyAttribute)));
            T B;

            if (compareProp != null)
            {
                B = (T)compareProp.GetValue(A);

                if (GetChangedProperties<T>(A, B).Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void StoreRecursive<T>(this ObservableCollection<T> col, SqlTransaction transaction, SqlConnection connection)
        {
            foreach (T i in col.Distinct())
            {
                if (i.Compare())
                {
                    i.Store(transaction, connection);
                }
            }
        }

        public static string StoredProcedureSave(this object obj)
        {
            Type type = obj.GetType();
            var sqlProp = type.GetFields().FirstOrDefault(x => Attribute.IsDefined(x, typeof(StoredProcSave)));

            if (sqlProp != null)
            {
                return sqlProp.GetValue(obj).ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string StoredProcedureDelete(this object obj)
        {
            Type type = obj.GetType();
            var sqlProp = type.GetFields().FirstOrDefault(x => Attribute.IsDefined(x, typeof(StoredProcDelete)));

            if (sqlProp != null)
            {
                return sqlProp.GetValue(obj).ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static List<string> GetChangedProperties<T>(object A, object B)
        {
            if (A != null && B != null)
            {
                var type = typeof(T);
                var allProperties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => !Attribute.IsDefined(x, typeof(SkipCompareAttribute)));
                var allSimpleProperties = allProperties.Where(pi => pi.PropertyType.IsSimpleType());

                var unequalProperties =
                       from pi in allSimpleProperties
                       let AValue = type.GetProperty(pi.Name).GetValue(A, null)
                       let BValue = type.GetProperty(pi.Name).GetValue(B, null)
                       where AValue != BValue && (AValue == null || !AValue.Equals(BValue))
                       select pi.Name;
                return unequalProperties.ToList();
            }
            else if (A != null && B == null)
            {
                return new List<string>() { "" };
            }
            else
            {
                throw new ArgumentNullException("You need to provide 2 non-null objects");
            }
        }

        public static bool IsSimpleType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return type.GetGenericArguments()[0].IsSimpleType();
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal)) || type.Equals(typeof(DateTime));
        }

        public static List<SqlParameter> ToSqlParamsList(this object obj, SqlParameter[] additionalParams = null)
        {
            var props = (
                from p in obj.GetType().GetProperties(System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
                let nameAttr = p.GetCustomAttributes(typeof(QueryParamNameAttribute), true)
                let ignoreAttr = p.GetCustomAttributes(typeof(QueryParamIgnoreAttribute), true)
                select new { Property = p, Names = nameAttr, Ignores = ignoreAttr }).ToList();

            var result = new List<SqlParameter>();

            foreach (var p in props)
            {
                if (p.Ignores != null && p.Ignores.Length > 0)
                    continue;

                var name = p.Names.FirstOrDefault() as QueryParamNameAttribute;
                var pinfo = new QueryParamInfo();

                if (name != null && !String.IsNullOrWhiteSpace(name.Name))
                    pinfo.Name = name.Name.Replace("@", "");
                else
                    pinfo.Name = p.Property.Name.Replace("@", "");

                pinfo.Value = p.Property.GetValue(obj) ?? DBNull.Value;
                var sqlParam = new SqlParameter(pinfo.Name, TypeConvertor.ToSqlDbType(p.Property.PropertyType))
                {
                    Value = pinfo.Value
                };

                result.Add(sqlParam);
            }

            if (additionalParams != null && additionalParams.Length > 0)
                result.AddRange(additionalParams);

            return result;

        }

        public static SqlParameter ToSqlPrimaryIDParam(this object obj, SqlParameter[] additionalParams = null)
        {
            Type type = obj.GetType();

            var sqlProp = type.GetProperties().FirstOrDefault(x => Attribute.IsDefined(x, typeof(PrimaryID)));

            if (sqlProp != null)
            {

            }
            else
            {
                throw new Exception("No Primary ID Field Defined");
            }

            var v = new QueryParamInfo();

            v.Name = sqlProp.Name;

            v.Value = sqlProp.GetValue(obj);


            var sqlParam = new SqlParameter(v.Name, TypeConvertor.ToSqlDbType(sqlProp.PropertyType))
            {
                Value = v.Value
            };

            return sqlParam;

        }

        private class QueryParamInfo
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        public class SQLParam : Attribute
        {
            private string variableName;
            private SqlDbType sqlType;
            private Type dataType;
            private object value;
            private bool required;

            public SQLParam(string variableName, SqlDbType sqlType, Type dataType, object value, bool required = false)
            {
                VariableName = variableName;
                SqlType = sqlType;
                DataType = dataType;
                Value = value;
                Required = required;
            }

            public string VariableName { get => variableName; set => variableName = value; }
            public SqlDbType SqlType { get => sqlType; set => sqlType = value; }
            public Type DataType { get => dataType; set => dataType = value; }
            public object Value { get => value; set => this.value = value; }
            public bool Required { get => required; set => required = value; }

        }
    }

    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static Object Copy(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }
        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);

            }
        }
        public static T Copy<T>(this T original)
        {
            return (T)Copy((Object)original);
        }



    }

    public static class ArrayExtensions
    {
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }

    public sealed class TypeConvertor
    {

        private struct DbTypeMapEntry
        {
            public Type Type;
            public DbType DbType;
            public SqlDbType SqlDbType;
            public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType)
            {
                this.Type = type;
                this.DbType = dbType;
                this.SqlDbType = sqlDbType;
            }

        };

        private static ArrayList _DbTypeList = new ArrayList();

        #region Constructors

        static TypeConvertor()
        {
            DbTypeMapEntry dbTypeMapEntry
            = new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Decimal), DbType.Decimal, SqlDbType.Decimal);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Int16), DbType.Int16, SqlDbType.SmallInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Int32), DbType.Int32, SqlDbType.Int);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Int64), DbType.Int64, SqlDbType.BigInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(float), DbType.Single, SqlDbType.Real);
            _DbTypeList.Add(dbTypeMapEntry);


        }

        private TypeConvertor()
        {

        }

        #endregion

        #region Methods       
        public static Type ToNetType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.Type;
        }

        public static Type ToNetType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.Type;
        }

        public static DbType ToDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.DbType;
        }

        public static DbType ToDbType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.DbType;
        }

        public static SqlDbType ToSqlDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.SqlDbType;
        }

        public static SqlDbType ToSqlDbType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.SqlDbType;
        }

        private static DbTypeMapEntry Find(Type type)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.Type == (Nullable.GetUnderlyingType(type) ?? type))
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                throw new ApplicationException("Referenced an unsupported Type " + type.ToString());
            }

            return (DbTypeMapEntry)retObj;
        }

        private static DbTypeMapEntry Find(DbType dbType)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.DbType == dbType)
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                throw
                new ApplicationException("Referenced an unsupported DbType " + dbType.ToString());
            }

            return (DbTypeMapEntry)retObj;
        }

        private static DbTypeMapEntry Find(SqlDbType sqlDbType)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.SqlDbType == sqlDbType)
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                throw
                new ApplicationException("Referenced an unsupported SqlDbType");
            }

            return (DbTypeMapEntry)retObj;
        }

        #endregion
    }

    [System.AttributeUsage(AttributeTargets.Property)]
    public class QueryParamNameAttribute : Attribute
    {
        public string Name { get; set; }
        public QueryParamNameAttribute(string name)
        {
            Name = name;
        }
    }

    [System.AttributeUsage(AttributeTargets.All)]
    public class StoredProcSave : Attribute
    {

    }

    [System.AttributeUsage(AttributeTargets.All)]
    public class StoredProcDelete : Attribute
    {

    }

    [System.AttributeUsage(AttributeTargets.Property)]
    public class QueryParamIgnoreAttribute : Attribute
    {
    }
    [System.AttributeUsage(AttributeTargets.Property)]
    public class SkipCompareAttribute : Attribute
    {

    }
    [System.AttributeUsage(AttributeTargets.Property)]
    public class PrimaryID : Attribute
    {

    }
    [System.AttributeUsage(AttributeTargets.Property)]
    public class OriginalCopyAttribute : Attribute
    {

    }
}
