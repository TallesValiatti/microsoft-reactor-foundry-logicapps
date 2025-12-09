namespace WeatherAgentApi;

public class Constants
{
    public static string OpenApiToolName = "OpenApiGetWeather";
    public static string OpenApiToolDescription = "Get weather in a specific location";

    public static string OpenApiToolSpec = """
   {
     "openapi": "3.0.3",
     "info": {
       "version": "1.0.0.0",
       "title": "logic-app-weather",
       "description": "Get the weather based on user's input"
     },
     "servers": [
       {
         "url": "https://prod-41.eastus.logic.azure.com/workflows/9914fd1d892c446191d0f60d3e758ca1/triggers/When_a_HTTP_request_is_received/paths"
       }
     ],
     "security": [
       {
         "sig": []
       }
     ],
     "paths": {
       "/invoke": {
         "post": {
           "description": "Get the weather based on user's input",
           "operationId": "When_a_HTTP_request_is_received-invoke",
           "parameters": [
             {
               "name": "api-version",
               "in": "query",
               "description": "`2016-10-01` is the most common generally available version",
               "required": true,
               "schema": {
                 "type": "string",
                 "default": "2016-10-01"
               },
               "example": "2016-10-01"
             },
             {
               "name": "sv",
               "in": "query",
               "description": "The version number",
               "required": true,
               "schema": {
                 "type": "string",
                 "default": "1.0"
               },
               "example": "1.0"
             },
             {
               "name": "sp",
               "in": "query",
               "description": "The permissions",
               "required": true,
               "schema": {
                 "type": "string",
                 "default": "%2Ftriggers%2FWhen_a_HTTP_request_is_received%2Frun"
               },
               "example": "%2Ftriggers%2FWhen_a_HTTP_request_is_received%2Frun"
             }
           ],
           "responses": {
             "200": {
               "description": "The Logic App Response.",
               "content": {
                 "application/json": {
                   "schema": {
                     "type": "object"
                   }
                 }
               }
             },
             "default": {
               "description": "The Logic App Response.",
               "content": {
                 "application/json": {
                   "schema": {
                     "type": "object"
                   }
                 }
               }
             }
           },
           "deprecated": false,
           "requestBody": {
             "content": {
               "application/json": {
                 "schema": {
                   "type": "object",
                   "properties": {
                     "location": {
                       "description": "Location for the weather",
                       "type": "string"
                     }
                   }
                 }
               }
             },
             "required": true
           }
         }
       }
     },
     "components": {
       "securitySchemes": {
         "sig": {
           "type": "apiKey",
           "description": "The SHA 256 hash of the entire request URI with an internal key.",
           "name": "sig",
           "in": "query"
         }
       }
     }
   }
   """;
}