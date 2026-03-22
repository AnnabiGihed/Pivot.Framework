using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Errors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Centralized catalog of reusable domain errors.
///              Exposes factories returning <see cref="Error"/> to integrate with CSharpFunctionalExtensions.
///              Provides two overload shapes:
///              - Default overloads (use standard resource keys)
///              - Descriptor overloads (require explicit code and message template descriptors)
///              Descriptor overloads intentionally do NOT use optional parameters to avoid ambiguous overload resolution.
/// </summary>
public static class BaseDomainErrors
{
	/// <summary>
	/// Creates an <see cref="Error"/> from an error code and a pre-formatted message.
	/// Centralises the <see cref="DomainError"/> → <see cref="Error"/> conversion that every
	/// factory method in this class requires.
	/// </summary>
	private static Error CreateError(string resourceKey, string formattedMessage)
	{
		var domainError = new DomainError(resourceKey, formattedMessage);
		return new Error(domainError.Code, domainError.Message);
	}

	/// <summary>
	/// General-purpose domain errors (CRUD, validation, length constraints, collections, etc.).
	/// </summary>
	public static class General
	{
		#region NotFound

		/// <summary>
		/// Creates a not-found error optionally scoped to an entity identifier.
		/// </summary>
		public static Error NotFound(Guid? id = null)
		{
			var forId = id is null ? "" : " " + string.Format(Resource.ForId, id);
			return CreateError(Resource.record_not_found, string.Format(Resource.RecordNotFound, forId));
		}

		/// <summary>
		/// Creates a not-found error using explicit code and message template descriptors.
		/// </summary>
		public static Error NotFound(Guid? id, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			var forId = id is null ? "" : " " + string.Format(Resource.ForId, id);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, forId));
		}

		#endregion NotFound

		#region ValueIsInvalid

		/// <summary>
		/// Creates an error indicating a value is invalid, optionally scoped to a field name and value.
		/// </summary>
		public static Error ValueIsInvalid(string? name = null, string? value = null)
			=> CreateError(Resource.value_is_invalid, string.Format(Resource.ValueIsInvalid, DomainError.ValueForField(name, value)));

		/// <summary>
		/// Creates an invalid-value error using explicit code and message template descriptors.
		/// </summary>
		public static Error ValueIsInvalid(string? name, string? value, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, DomainError.ValueForField(name, value)));
		}

		#endregion ValueIsInvalid

		#region ValueAlreadyExists

		/// <summary>
		/// Creates an error indicating a value already exists, optionally scoped to a field name and value.
		/// </summary>
		public static Error ValueAlreadyExists(string? name = null, string? value = null)
			=> CreateError(Resource.value_already_exists, string.Format(Resource.ValueAlreadyExists, DomainError.ValueForField(name, value)));

		/// <summary>
		/// Creates an already-exists error using explicit code and message template descriptors.
		/// </summary>
		public static Error ValueAlreadyExists(string? name, string? value, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, DomainError.ValueForField(name, value)));
		}

		#endregion ValueAlreadyExists

		#region ValueIsToLong

		/// <summary>
		/// Creates an error indicating a value is too long.
		/// </summary>
		[Obsolete("Use ValueIsTooLong instead.", false)]
		public static Error ValueIsToLong(string? name = null, string? value = null, int maxLength = 0)
			=> CreateError(Resource.value_is_tolong, string.Format(Resource.ValueIsToLong, DomainError.ValueForField(name, value), maxLength));

		/// <summary>
		/// Creates an error indicating a value is too long.
		/// </summary>
		public static Error ValueIsTooLong(string? name = null, string? value = null, int maxLength = 0)
		{
#pragma warning disable CS0618 // Intentional call to obsolete forwarder
			return ValueIsToLong(name, value, maxLength);
#pragma warning restore CS0618
		}

		/// <summary>
		/// Creates a too-long error using explicit code and message template descriptors.
		/// </summary>
		[Obsolete("Use ValueIsTooLong instead.", false)]
		public static Error ValueIsToLong(string? name, string? value, int maxLength, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, DomainError.ValueForField(name, value), maxLength));
		}

		/// <summary>
		/// Creates a too-long error using explicit code and message template descriptors.
		/// </summary>
		public static Error ValueIsTooLong(string? name, string? value, int maxLength, string codeDescriptor, string propertyDescriptor)
		{
#pragma warning disable CS0618 // Intentional call to obsolete forwarder
			return ValueIsToLong(name, value, maxLength, codeDescriptor, propertyDescriptor);
#pragma warning restore CS0618
		}

		#endregion ValueIsToLong

		#region ValueIsToShort

		/// <summary>
		/// Creates an error indicating a value is too short.
		/// </summary>
		[Obsolete("Use ValueIsTooShort instead.", false)]
		public static Error ValueIsToShort(string? name = null, string? value = null, int minLength = 0)
			=> CreateError(Resource.value_is_toshort, string.Format(Resource.ValueIsToShort, DomainError.ValueForField(name, value), minLength));

		/// <summary>
		/// Creates an error indicating a value is too short.
		/// </summary>
		public static Error ValueIsTooShort(string? name = null, string? value = null, int minLength = 0)
		{
#pragma warning disable CS0618 // Intentional call to obsolete forwarder
			return ValueIsToShort(name, value, minLength);
#pragma warning restore CS0618
		}

		/// <summary>
		/// Creates a too-short error using explicit code and message template descriptors.
		/// </summary>
		[Obsolete("Use ValueIsTooShort instead.", false)]
		public static Error ValueIsToShort(string? name, string? value, int minLength, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, DomainError.ValueForField(name, value), minLength));
		}

		/// <summary>
		/// Creates a too-short error using explicit code and message template descriptors.
		/// </summary>
		public static Error ValueIsTooShort(string? name, string? value, int minLength, string codeDescriptor, string propertyDescriptor)
		{
#pragma warning disable CS0618 // Intentional call to obsolete forwarder
			return ValueIsToShort(name, value, minLength, codeDescriptor, propertyDescriptor);
#pragma warning restore CS0618
		}

		#endregion ValueIsToShort

		#region ValueIsRequired

		/// <summary>
		/// Creates an error indicating a value is required, optionally scoped to a field name.
		/// </summary>
		public static Error ValueIsRequired(string? name = null)
			=> CreateError(Resource.value_is_required, string.Format(Resource.Required, DomainError.ForField(name)));

		/// <summary>
		/// Creates a required-value error using explicit code and message template descriptors.
		/// </summary>
		public static Error ValueIsRequired(string? name, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, DomainError.ForField(name)));
		}

		#endregion ValueIsRequired

		#region ValueNotNegative

		/// <summary>
		/// Creates an error indicating a value must not be negative.
		/// </summary>
		public static Error ValueNotNegative(string? name = null, string? value = null)
			=> CreateError(Resource.value_not_negative, string.Format(Resource.ValueIsNegative, DomainError.ValueForField(name, value)));

		/// <summary>
		/// Creates a not-negative error using explicit code and message template descriptors.
		/// </summary>
		public static Error ValueNotNegative(string? name, string? value, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, DomainError.ValueForField(name, value)));
		}

		#endregion ValueNotNegative

		#region InvalidLength

		/// <summary>
		/// Creates an error indicating an invalid string length.
		/// </summary>
		public static Error InvalidLength(string? name = null)
			=> CreateError(Resource.invalid_string_length, string.Format(Resource.InvalidLength, DomainError.ForField(name)));

		/// <summary>
		/// Creates an invalid-length error using explicit code and message template descriptors.
		/// </summary>
		public static Error InvalidLength(string? name, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, DomainError.ForField(name)));
		}

		#endregion InvalidLength

		#region CollectionIsTooSmall

		/// <summary>
		/// Creates an error indicating a collection has too few items.
		/// </summary>
		public static Error CollectionIsTooSmall(int min, int current)
			=> CreateError(Resource.collection_is_too_small, string.Format(Resource.CollectionIsToSmall, min, current));

		/// <summary>
		/// Creates a too-small collection error using explicit code and message template descriptors.
		/// </summary>
		public static Error CollectionIsTooSmall(int min, int current, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, min, current));
		}

		#endregion CollectionIsTooSmall

		#region CollectionIsTooLarge

		/// <summary>
		/// Creates an error indicating a collection has too many items.
		/// </summary>
		public static Error CollectionIsTooLarge(int max, int current)
			=> CreateError(Resource.collection_is_too_large, string.Format(Resource.CollectionIsTooLarge, max, current));

		/// <summary>
		/// Creates a too-large collection error using explicit code and message template descriptors.
		/// </summary>
		public static Error CollectionIsTooLarge(int max, int current, string codeDescriptor, string propertyDescriptor)
		{
			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, max, current));
		}

		#endregion CollectionIsTooLarge

		#region UnexpectedError

		/// <summary>
		/// Creates an unexpected error with a provided message.
		/// </summary>
		public static Error UnexpectedError(string message)
		{
			if (message is null)
				throw new ArgumentNullException(nameof(message));

			return CreateError(Resource.internal_server_error, message);
		}

		/// <summary>
		/// Creates an unexpected error using explicit code and message template descriptors.
		/// </summary>
		public static Error UnexpectedError(string message, string codeDescriptor, string propertyDescriptor)
		{
			if (message is null)
				throw new ArgumentNullException(nameof(message));

			DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
			return CreateError(codeDescriptor, string.Format(propertyDescriptor, message));
		}

		#endregion UnexpectedError
	}

	/// <summary>
	/// Domain errors related to value objects.
	/// </summary>
	[Obsolete("Domain-specific errors should be defined in the consuming domain, not in the framework.", false)]
	public static class ValueObjects
	{
		/// <summary>
		/// Errors related to the Money value object.
		/// </summary>
		public static class Money
		{
			#region CurrencyMismatch

			/// <summary>
			/// Creates an error indicating currencies do not match.
			/// </summary>
			public static Error CurrencyMismatch(string? name, string? value1, string? value2)
				=> CreateError(Resource.value_is_invalid,
					string.Format(Resource.CurrencyMismatch,
						DomainError.ValueForField(name, $"{value1},{value2}")));

			/// <summary>
			/// Creates a currency-mismatch error using explicit code and message template descriptors.
			/// </summary>
			public static Error CurrencyMismatch(string? name, string? value1, string? value2, string codeDescriptor, string propertyDescriptor)
			{
				DomainError.ValidateDescriptors(codeDescriptor, propertyDescriptor);
				return CreateError(codeDescriptor,
					string.Format(propertyDescriptor,
						DomainError.ValueForField(name, $"{value1},{value2}")));
			}

			#endregion CurrencyMismatch
		}
	}
}
