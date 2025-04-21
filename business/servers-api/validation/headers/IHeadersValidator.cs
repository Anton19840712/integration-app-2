﻿using servers_api.models.response;

namespace servers_api.validation.headers
{
	public interface IHeadersValidator
	{
		Task<ResponseIntegration> ValidateHeadersAsync(IHeaderDictionary headers);
	}
}
