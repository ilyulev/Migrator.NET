using System;
using System.Collections.Generic;
using System.Data;
using Migrator.Framework;

namespace Migrator.Providers
{
	/// <summary>
	/// Defines the implementations specific details for a particular database.
	/// </summary>
	public abstract class Dialect
	{
		readonly Dictionary<ColumnProperty, string> propertyMap = new Dictionary<ColumnProperty, string>();
		readonly HashSet<string> reservedWords = new HashSet<string>();
		readonly TypeNames typeNames = new TypeNames();
		readonly List<DbType> unsignedCompatibleTypes = new List<DbType>();

		protected Dialect()
		{
			RegisterProperty(ColumnProperty.Null, "NULL");
			RegisterProperty(ColumnProperty.NotNull, "NOT NULL");
			RegisterProperty(ColumnProperty.Unique, "UNIQUE");
			RegisterProperty(ColumnProperty.PrimaryKey, "PRIMARY KEY");
		}

		public virtual bool ColumnNameNeedsQuote
		{
			get { return false; }
		}

		public virtual bool TableNameNeedsQuote
		{
			get { return false; }
		}

		public virtual bool ConstraintNameNeedsQuote
		{
			get { return false; }
		}

		public virtual bool IdentityNeedsType
		{
			get { return true; }
		}

		public virtual bool NeedsNotNullForIdentity
		{
			get { return true; }
		}

		public virtual bool SupportsIndex
		{
			get { return true; }
		}

		public virtual string QuoteTemplate
		{
			get { return "\"{0}\""; }
		}

		protected void AddReservedWord(string reservedWord)
		{
			reservedWords.Add(reservedWord.ToUpperInvariant());
		}

		protected void AddReservedWords(params string[] words)
		{
			if (words == null) return;
			foreach (string word in words) reservedWords.Add(word);
		}

		public virtual bool IsReservedWord(string reservedWord)
		{
			if (string.IsNullOrEmpty(reservedWord)) throw new ArgumentNullException("reservedWord");

			if (reservedWords == null) return false;

			bool isReserved = reservedWords.Contains(reservedWord.ToUpperInvariant());

			if (isReserved)
			{
				Console.WriteLine("Reserved word: {0}", reservedWord);
			}

			return isReserved;
		}

		public abstract ITransformationProvider GetTransformationProvider(Dialect dialect, string connectionString);

		public ITransformationProvider NewProviderForDialect(string connectionString)
		{
			return GetTransformationProvider(this, connectionString);
		}

		/// <summary>
		/// Subclasses register a typename for the given type code and maximum
		/// column length. <c>$l</c> in the type name will be replaced by the column
		/// length (if appropriate)
		/// </summary>
		/// <param name="code">The typecode</param>
		/// <param name="capacity">Maximum length of database type</param>
		/// <param name="name">The database type name</param>
		protected void RegisterColumnType(DbType code, int capacity, string name)
		{
			typeNames.Put(code, capacity, name);
		}

		/// <summary>
		/// Suclasses register a typename for the given type code. <c>$l</c> in the 
		/// typename will be replaced by the column length (if appropriate).
		/// </summary>
		/// <param name="code">The typecode</param>
		/// <param name="name">The database type name</param>
		protected void RegisterColumnType(DbType code, string name)
		{
			typeNames.Put(code, name);
		}

		public ColumnPropertiesMapper GetColumnMapper(Column column)
		{
			string type = column.Size > 0 ? GetTypeName(column.Type, column.Size) : GetTypeName(column.Type);
			if (! IdentityNeedsType && column.IsIdentity)
				type = String.Empty;

			return new ColumnPropertiesMapper(this, type);
		}

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		public virtual string GetTypeName(DbType type)
		{
			string result = typeNames.Get(type);
			if (result == null)
			{
				throw new Exception(string.Format("No default type mapping for DbType {0}", type));
			}

			return result;
		}

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		/// <param name="length"></param>
		public virtual string GetTypeName(DbType type, int length)
		{
			return GetTypeName(type, length, 0, 0);
		}

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		/// <param name="length"></param>
		/// <param name="precision"></param>
		/// <param name="scale"></param>
		public virtual string GetTypeName(DbType type, int length, int precision, int scale)
		{
			string resultWithLength = typeNames.Get(type, length, precision, scale);
			if (resultWithLength != null)
				return resultWithLength;

			return GetTypeName(type);
		}

		public void RegisterProperty(ColumnProperty property, string sql)
		{
			if (! propertyMap.ContainsKey(property))
			{
				propertyMap.Add(property, sql);
			}
			propertyMap[property] = sql;
		}

		public string SqlForProperty(ColumnProperty property)
		{
			if (propertyMap.ContainsKey(property))
			{
				return propertyMap[property];
			}
			return String.Empty;
		}

		public virtual string Quote(string value)
		{
			return String.Format(QuoteTemplate, value);
		}

		public virtual string Default(object defaultValue)
		{
			return String.Format("DEFAULT {0}", defaultValue);
		}

		public ColumnPropertiesMapper GetAndMapColumnProperties(Column column)
		{
			ColumnPropertiesMapper mapper = GetColumnMapper(column);
			mapper.MapColumnProperties(column);
			if (column.DefaultValue != null)
				mapper.Default = column.DefaultValue;
			return mapper;
		}

		/// <summary>
		/// Subclasses register which DbTypes are unsigned-compatible (ie, available in signed and unsigned variants)
		/// </summary>
		/// <param name="type"></param>
		protected void RegisterUnsignedCompatible(DbType type)
		{
			unsignedCompatibleTypes.Add(type);
		}

		/// <summary>
		/// Determine if a particular database type has an unsigned variant
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>True if the database type has an unsigned variant, otherwise false</returns>
		public bool IsUnsignedCompatible(DbType type)
		{
			return unsignedCompatibleTypes.Contains(type);
		}

	}
}