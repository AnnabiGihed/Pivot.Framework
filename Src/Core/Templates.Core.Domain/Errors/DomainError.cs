using CSharpFunctionalExtensions;
using Templates.Core.Domain.Shared;

namespace Templates.Core.Domain.Errors;

public sealed partial class DomainError : ValueObject<DomainError>
{
	private const string Separator = "||";

	public string Code { get; }
	public string Message { get; }

	public DomainError(string code, string message)
	{
		Code = code;
		Message = message;
	}

	public string Serialize()
	{
		return $"{Code}{Separator}{Message}";
	}

	public static Error Deserialize(string serialized)
	{
		if (serialized == $"{Resource.ANoneEmptyRequestBodyIsRequired}")
			return BaseDomainErrors.General.ValueIsRequired(null);

		string[] data = serialized.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

		if (data.Length < 2)
			throw new Exception($"{Resource.InvalidErrorSerialization}:'{serialized}'");

		return new Error(data[0], data[1]);
	}

	protected override bool EqualsCore(DomainError other)
	{
		return Code == other.Code;
	}

	protected override int GetHashCodeCore()
	{
		return Code.GetHashCode();
	}

	public static string ForField(string? name = null)
	{
		return name == null ?
		" "
		:
			" " + string.Format($"{Resource.ForField}", name) + " ";

	}

	public static string ValueForField(string? name = null, string? value = null)
	{
		string label = " ";

		if (name != null && value != null)
		{
			label += string.Format($"{Resource.ValueForField}", value, name) + " ";
		}
		else if (name != null)
		{
			label += name + " ";
		}
		else if (value != null)
		{
			label += value + " ";
		}

		return label;
	}
}

public static class BaseDomainErrors
{
	public static class General
	{
		#region NotFound

		private static DomainError NotFoundDomainError(Guid? id = null)
		{
			string forId = id == null ? "" : " " + string.Format($"{Resource.ForId}", id);
			return new($"{Resource.record_not_found}", string.Format($"{Resource.RecordNotFound}", forId));
		}

		public static Error NotFound(Guid? id = null)
		{
			DomainError domainError = BaseDomainErrors.General.NotFoundDomainError(id);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError NotFoundDomainError(Guid? id = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			string forId = id == null ? "" : " " + string.Format($"{Resource.ForId}", id);
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", forId));
		}

		public static Error NotFound(Guid? id = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.NotFoundDomainError(id, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion NotFound

		#region ValueIsInvalid

		private static DomainError ValueIsInvalidDomainError(string? name = null, string? value = null)
		{

			return new($"{Resource.value_is_invalid}", string.Format($"{Resource.ValueIsInvalid}", DomainError.ValueForField(name, value)));
		}

		public static Error ValueIsInvalid(string? name = null, string? value = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsInvalidDomainError(name, value);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError ValueIsInvalidDomainError(string? name = null, string? value = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{

			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", DomainError.ValueForField(name, value)));
		}

		public static Error ValueIsInvalid(string? name = null, string? value = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsInvalidDomainError(name, value, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion ValueIsInvalid

		#region ValueAlreadyExists

		private static DomainError ValueAlreadyExistsDomainError(string? name = null, string? value = null)
		{
			return new($"{Resource.value_already_exists}", string.Format($"{Resource.ValueAlreadyExists}", DomainError.ValueForField(name, value)));
		}

		public static Error ValueAlreadyExists(string? name = null, string? value = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueAlreadyExistsDomainError(name, value);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError ValueAlreadyExistsDomainError(string? name = null, string? value = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", DomainError.ValueForField(name, value)));
		}

		public static Error ValueAlreadyExists(string? name = null, string? value = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueAlreadyExistsDomainError(name, value, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion ValueAlreadyExists

		#region ValueIsToLong

		private static DomainError ValueIsToLongDomainError(string? name = null, string? value = null, int maxLength = 0)
		{

			return new($"{Resource.value_is_tolong}", string.Format($"{Resource.ValueIsToLong}", DomainError.ValueForField(name, value), maxLength));
		}

		public static Error ValueIsToLong(string? name = null, string? value = null, int maxLength = 0)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsToLongDomainError(name, value, maxLength);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError ValueIsToLongDomainError(string? name = null, string? value = null, int maxLength = 0, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", DomainError.ValueForField(name, value), maxLength));
		}

		public static Error ValueIsToLong(string? name = null, string? value = null, int maxLength = 0, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsToLongDomainError(name, value, maxLength, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion ValueIsToLong

		#region ValueIsToShort

		private static DomainError ValueIsToShortDomainError(string? name = null, string? value = null, int minLength = 0)
		{

			return new($"{Resource.value_is_toshort}", string.Format($"{Resource.ValueIsToShort}", DomainError.ValueForField(name, value), minLength));
		}

		public static Error ValueIsToShort(string? name = null, string? value = null, int minLength = 0)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsToShortDomainError(name, value, minLength);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError ValueIsToShortDomainError(string? name = null, string? value = null, int minLength = 0, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", DomainError.ValueForField(name, value), minLength));
		}

		public static Error ValueIsToShort(string? name = null, string? value = null, int minLength = 0, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsToShortDomainError(name, value, minLength, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}


		#endregion ValueIsToShort

		#region ValueIsRequired

		private static DomainError ValueIsRequiredDomainError(string? name = null)
		{
			return new($"{Resource.value_is_required}", string.Format($"{Resource.Required}", DomainError.ForField(name)));
		}

		public static Error ValueIsRequired(string? name = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsRequiredDomainError(name);
			return new Error(domainError.Code, domainError.Message);

		}

		private static DomainError ValueIsRequiredDomainError(string? name = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", DomainError.ForField(name)));
		}

		private static Error ValueIsRequired(string? name = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueIsRequiredDomainError(name, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion ValueIsRequired

		#region ValueNotNegative
		private static DomainError ValueNotNegativeDomainError(string? name = null, string? value = null)
		{
			return new($"{Resource.value_not_negative}", string.Format($"{Resource.ValueIsNegative}", DomainError.ValueForField(name, value)));
		}

		public static Error ValueNotNegative(string? name = null, string? value = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueNotNegativeDomainError(name, value);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError ValueNotNegativeDomainError(string? name = null, string? value = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", DomainError.ValueForField(name, value)));
		}

		public static Error ValueNotNegative(string? name = null, string? value = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.ValueNotNegativeDomainError(name, value, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion ValueNotNegative

		#region InvalidLength
		private static DomainError InvalidLengthDomainError(string? name = null)
		{
			return new($"{Resource.invalid_string_length}", string.Format($"{Resource.InvalidLength}", DomainError.ForField(name)));
		}

		public static Error InvalidLength(string? name = null)
		{
			DomainError domainError = BaseDomainErrors.General.InvalidLengthDomainError(name);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError InvalidLengthDomainError(string? name = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", DomainError.ForField(name)));
		}

		public static Error InvalidLength(string? name = null, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.InvalidLengthDomainError(name, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion InvalidLength

		#region CollectionIsTooSmall

		private static DomainError CollectionIsTooSmallDomainError(int min, int current)
		{
			return new($"{Resource.collection_is_too_small}", string.Format($"{Resource.CollectionIsToSmall}", min, current));
		}

		public static Error CollectionIsTooSmall(int min, int current)
		{
			DomainError domainError = BaseDomainErrors.General.CollectionIsTooSmallDomainError(min, current);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError CollectionIsTooSmallDomainError(int min, int current, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", min, current));
		}

		public static Error CollectionIsTooSmall(int min, int current, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.CollectionIsTooSmallDomainError(min, current, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion CollectionIsTooSmall

		#region CollectionIsTooLarge
		private static DomainError CollectionIsTooLargeDomainError(int max, int current)
		{
			return new($"{Resource.collection_is_too_large}", string.Format($"{Resource.CollectionIsTooLarge}", max, current));
		}

		public static Error CollectionIsTooLarge(int max, int current)
		{
			DomainError domainError = BaseDomainErrors.General.CollectionIsTooLargeDomainError(max, current);
			return new Error(domainError.Code, domainError.Message);
		}

		private static DomainError CollectionIsTooLargeDomainError(int max, int current, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			return new($"{codeDescriptor}", string.Format($"{propertyDescriptor}", max, current));
		}

		public static Error CollectionIsTooLarge(int max, int current, string? codeDescriptor = null, string? propertyDescriptor = null)
		{
			DomainError domainError = BaseDomainErrors.General.CollectionIsTooLargeDomainError(max, current, codeDescriptor, propertyDescriptor);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion CollectionIsTooLarge

		#region InternalServerError
		private static DomainError InternalServerErrorDomainError(string message)
		{
			return new($"{Resource.internal_server_error}", message);
		}

		public static Error InternalServerError(string message)
		{
			DomainError domainError = BaseDomainErrors.General.InternalServerErrorDomainError(message);
			return new Error(domainError.Code, domainError.Message);
		}

		#endregion InternalServerError
	}

	public static class ValueObjects
	{
		public static class Money
		{
			#region CurrencyMismatch

			private static DomainError CurrencyMismatchDomainError(string? name, string? value1, string? value2)
			{
				return new($"{Resource.value_is_invalid}", string.Format($"{Resource.CurrencyMismatch}", DomainError.ValueForField(name, $"{value1},{value2}")));
			}

			public static Error CurrencyMismatch(string? name, string? value1, string? value2)
			{
				DomainError domainError = BaseDomainErrors.ValueObjects.Money.CurrencyMismatchDomainError(name, value1, value2);
				return new Error(domainError.Code, domainError.Message);
			}

			#endregion CurrencyMismatch
		}
	}
}