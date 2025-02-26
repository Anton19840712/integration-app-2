﻿using servers_api.models.internallayer.common;
using servers_api.models.response;

namespace servers_api.Services.InternalSystems;

public interface ITeachService
{
	Task<ResponseIntegration> TeachBPMNAsync(
		CombinedModel model,
		CancellationToken token);
}