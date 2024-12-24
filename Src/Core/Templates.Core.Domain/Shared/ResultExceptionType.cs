using System.ComponentModel;

namespace Templates.Core.Domain.Shared;

[DefaultValue(BadRequest)]
public enum ResultExceptionType
{
	BadRequest,
	NotFound,
	None
}
