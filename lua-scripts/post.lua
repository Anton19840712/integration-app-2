local http = require("socket.http")
local ltn12 = require("ltn12")
local json = require("dkjson") -- JSON библиотека для Lua

-- Создаем JSON-объект
local jsonData = {
    protocol = "TCP",
    dataFormat = "json",
    companyName = "corporation",
    model = {
        InternalModel = "{\"shipmentId\":\"12345\",\"destination\":\"New York\",\"weightKg\":250,\"status\":\"In Transit\",\"estimatedDelivery\":\"2025-01-25\"}"
    },
    QueuesNames = {
        InQueueName = "corporation_in",
        OutQueueName = "corporation_out"
    },
    dataOptions = {
        client = false,
        server = true,
        serverDetails = {
            host = "127.0.0.1",
            port = 5018
        }
    }
}


-- Преобразуем таблицу в JSON
local jsonString = json.encode(jsonData)

-- Отправляем POST-запрос
local response = {}
local _, code = http.request{
    url = "http://localhost:5106/api/servers/upload",
    method = "POST",
    headers = {
        ["Content-Type"] = "application/json",
        ["Content-Length"] = tostring(#jsonString)
    },
    source = ltn12.source.string(jsonString),
    sink = ltn12.sink.table(response)
}

-- Проверяем результат
if code == 200 then
    print("JSON успешно отправлен!")
else
    print("Ошибка отправки! Код ответа: " .. tostring(code))
end
