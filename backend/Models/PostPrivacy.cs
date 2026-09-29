using System.Text.Json.Serialization;

namespace Conectando.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PostPrivacy
{
    Public = 0,
    Friends = 1,
    Private = 2,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PostMediaState
{
    Pending = 0,
    Attached = 1,
}