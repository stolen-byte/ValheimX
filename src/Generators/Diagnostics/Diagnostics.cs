// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class Diagnostics
{
	public static EquatableArray<Diagnostic> Empty = EquatableArray.Empty<Diagnostic>();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EquatableArray<Diagnostic> From(params Diagnostic[] diagnostics)
		=> new(diagnostics);

	// =============================================================================
	static class Descriptors
	{
		const string Category = "ValheimX";
		const string ID = "VXLIB";

		public static readonly DiagnosticDescriptor MultipleModsDefined = new(
			$"{ID}001",
			$"Multiple types marked with {TypeNames.Internal.ModAttribute}",
			"Only a single type may be marked as a Mod",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor TypeNotStatic = new(
			$"{ID}002",
			"Type must be static",
			"Type '{0}' must be declared static",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor MethodNotStatic = new(
			$"{ID}003",
			"Method must be static",
			"Method '{0}' must be declared static",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor ReadOnlyConfigEntry = new(
			$"{ID}004",
			"Config value must be writable",
			"Config value '{0}' must be writable",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor InvalidParameterType = new(
			$"{ID}005",
			"Config parameter type mismatch",
			"{0} value of '{1}' should be of type {2}",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor InvalidRangeType = new(
			$"{ID}006",
			"Config value cannot have Min/Max parameters",
			"Config value '{0}' of type {1} cannot have min/max parameters",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor MissingConfigValueAttribute = new(
			$"{ID}007",
			"Config entry should have a [ConfigValue] attribute",
			"Config entry '{0}' is missing a [ConfigValue] attribute, and will have a default key & empty description",
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor MultipleMethodsDefined = new(
			$"{ID}008",
			"Only a single method with this name should be defined",
			"Multiple methods found with the name '{0}'",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor MissingExecMethod = new(
			$"{ID}009",
			"Unable to find the named Execute method",
			"No method named '{0}' in command {1}",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor ExecMethodMissingCtxParameter = new(
			$"{ID}010",
			"Terminal command's first argument has invalid type",
			"Method '{0}'s first parameter should be of type {1}",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor ExecMethodInvalidReturnType = new(
			$"{ID}011",
			"Terminal command execute method has invalid return type",
			"Method '{0}'s return type should be either void, string, or bool",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor ExecMethodInvalidArgumentType = new(
			$"{ID}012",
			"Terminal commands can only have arguments with primitive types",
			"Argument '{0}' of command '{1}' has invalid type, must be one of {2}",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor MissingTabCompleteMethod = new(
			$"{ID}013",
			"Command tab completion method was named, but not found",
			"No method named '{0}' found in command '{1}'",
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor TabCompleteInvalidArgs = new(
			$"{ID}014",
			"Command tab completion method should have zero arguments",
			"Method '{0}' should have no arguments",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor TabCompleteInvalidReturnType = new(
			$"{ID}015",
			"Command tab completion method has invalid return type",
			"Method '{0}' should return {1}",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);
	}

	// =============================================================================
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MultipleModsDefined(SourceLocation? location)
		=> new(Descriptors.MultipleModsDefined, location);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic TypeNotStatic(ISymbol type)
		=> new(Descriptors.TypeNotStatic, type.SourceLocation(), type.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MethodNotStatic(IMethodSymbol method)
		=> new(Descriptors.MethodNotStatic, method.SourceLocation(), method.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic ReadOnlyConfigValue(ISymbol member)
		=> new(Descriptors.ReadOnlyConfigEntry, member.SourceLocation(), member.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic InvalidParameterType(string name, ISymbol sym, ITypeSymbol type)
		=> new(Descriptors.InvalidParameterType, sym.SourceLocation(), name, sym.Name, type.ToDisplayString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic InvalidRangeType(ISymbol member, ITypeSymbol type)
		=> new(Descriptors.InvalidRangeType, member.SourceLocation(), member.Name, type.ToDisplayString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MissingConfigValueAttribute(ISymbol member)
		=> new(Descriptors.MissingConfigValueAttribute, member.SourceLocation(), member.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MultipleMethodsDefined(IMethodSymbol method)
		=> new(Descriptors.MultipleMethodsDefined, method.SourceLocation(), method.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MissingExecMethod(string name, INamedTypeSymbol container)
		=> new(Descriptors.MissingExecMethod, container.SourceLocation(), name, container.ToDisplayString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic ExecMethodMissingCtxParameter(IMethodSymbol method, INamedTypeSymbol termType)
		=> new(Descriptors.ExecMethodMissingCtxParameter, method.SourceLocation(), method.Name, termType.ToDisplayString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic ExecMethodInvalidReturnType(IMethodSymbol method)
		=> new(Descriptors.ExecMethodInvalidReturnType, method.SourceLocation(), method.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic ExecMethodInvalidArgumentType(IParameterSymbol param, string commandName)
		=> new(Descriptors.ExecMethodInvalidArgumentType, param.SourceLocation(), param.Name, commandName, ConsoleCommandGenerator.ValidArgTypesString);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MissingTabCompleteMethod(string name, INamedTypeSymbol container)
		=> new(Descriptors.MissingTabCompleteMethod, container.SourceLocation(), name, container.ToDisplayString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic TabCompleteInvalidArgs(IMethodSymbol method)
		=> new(Descriptors.TabCompleteInvalidArgs, method.SourceLocation(), method.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic TabCompleteInvalidReturnType(IMethodSymbol method, INamedTypeSymbol expectedType)
		=> new(Descriptors.TabCompleteInvalidReturnType, method.SourceLocation(), method.Name, expectedType.ToDisplayString());
}
