// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class Reflection
{
	public static class Flags
	{
		public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
		public const BindingFlags PublicInstance = BindingFlags.Instance | BindingFlags.Public;

		public const BindingFlags All = BindingFlags.Public
					| BindingFlags.NonPublic
					| BindingFlags.Instance
					| BindingFlags.Static;

		public const BindingFlags AllDeclared = All | BindingFlags.DeclaredOnly;
	}

	// Flags.All is not sufficient enough to return private members from base classes
	public static T? FindInInheritanceChain<T>(Type type, Func<Type, T?> selector)
		where T : class
	{
		var t = type;
		while (t is not null) {
			var result = selector(t);
			if (result is not null)
				return result;
			t = t.BaseType;
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FieldInfo? Field(Type type, string name)
		=> FindInInheritanceChain(type, t => t.GetField(name, Flags.All));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FieldInfo? Field<T>(string name)
		=> Field(typeof(T), name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FieldInfo? Field(Type type, Func<FieldInfo, bool> selector)
		=> FindInInheritanceChain(type, t => t.GetFields(Flags.All).FirstOrDefault(selector));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FieldInfo? Field<T>(Func<FieldInfo, bool> selector)
		=> Field(typeof(T), selector);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FieldInfo[] Fields(Type type)
		=> type.GetFields(Flags.AllDeclared);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FieldInfo[] Fields<T>()
		=> Fields(typeof(T));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyInfo? Property(Type type, string name)
		=> FindInInheritanceChain(type, t => t.GetProperty(name, Flags.All));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyInfo? Property<T>(string name)
		=> Property(typeof(T), name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyInfo? Property(Type type, Func<PropertyInfo, bool> selector)
		=> FindInInheritanceChain(type, t => t.GetProperties(Flags.All).FirstOrDefault(selector));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyInfo? Property<T>(Func<PropertyInfo, bool> selector)
		=> Property(typeof(T), selector);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyInfo[] Properties(Type type)
		=> type.GetProperties(Flags.AllDeclared);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyInfo[] Properties<T>()
		=> Properties(typeof(T));

	public static MethodInfo? Method(Type type, string name)
	{
		try {
			return FindInInheritanceChain(type, t => t.GetMethod(name, Flags.All));
		} catch (AmbiguousMatchException) {
			return Method(type, name, []);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MethodInfo? Method<T>(string name)
		=> Method(typeof(T), name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MethodInfo? Method(Type type, string name, params Type[] parameters)
		=> FindInInheritanceChain(type, t => t.GetMethod(name, Flags.All, null, parameters, []));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MethodInfo? Method<T>(string name, params Type[] parameters)
		=> Method(typeof(T), name, parameters);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MethodInfo? Method(Type type, Func<MethodInfo, bool> selector)
		=> FindInInheritanceChain(type, t => t.GetMethods(Flags.All).FirstOrDefault(selector));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MethodInfo? Method<T>(Func<MethodInfo, bool> selector)
		=> Method(typeof(T), selector);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MethodInfo[] Methods(Type type)
		=> type.GetMethods(Flags.AllDeclared);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MethodInfo[] Methods<T>()
		=> Methods(typeof(T));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConstructorInfo? Constructor(Type type, params Type[] parameters)
		=> type.GetConstructor(Flags.AllDeclared & ~BindingFlags.Static, null, parameters, []);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConstructorInfo? Constructor<T>(params Type[] parameters)
		=> Constructor(typeof(T), parameters);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConstructorInfo? Constructor(Type type, Func<ConstructorInfo, bool> selector)
		=> FindInInheritanceChain(type, t => t.GetConstructors(Flags.All).FirstOrDefault(selector));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConstructorInfo? Constructor<T>(Func<ConstructorInfo, bool> selector)
		=> Constructor(typeof(T), selector);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConstructorInfo[] Constructors(Type type)
		=> type.GetConstructors(Flags.AllDeclared & ~BindingFlags.Static);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConstructorInfo[] Constructors<T>()
		=> Constructors(typeof(T));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EventInfo? Event(Type type, string name)
		=> type.GetEvent(name, Flags.All);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EventInfo? Event<T>(string name)
		=> Event(typeof(T), name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EventInfo[] Events(Type type)
		=> type.GetEvents(Flags.All);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EventInfo[] Events<T>()
		=> Events(typeof(T));

	public static void AddEventHandler<T, THandler>(T instance, Type type, string name, THandler handler)
		where THandler : Delegate
	{
		if (Event(type, name) is not { } ev)
			throw new InvalidOperationException($"no event named '{name}' found on type '{type.FullName}'");
		ev.AddEventHandler(instance, handler);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddEventHandler<T, THandler>(T instance, string name, THandler handler)
		where T : notnull
		where THandler : Delegate
		=> AddEventHandler(instance, instance.GetType(), name, handler);
}
