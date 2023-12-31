﻿using System.Text.Json.Serialization;

namespace External.Mock.API.Service.Mulesoft.Model.Response
{
    public class OAuthResponse
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; } = "Bearer";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; } = 3599;

        [JsonPropertyName("ext_expires_in")]
        public int ExtExpiresIn { get; set; } = 3599;

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; } = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiJhcGk6Ly9jb20udmFsdWVyZXRhaWwubWFwaS53YXBlLmV4cGVyaWVuY2Uuc2l0IiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvOGE4NjUzMjEtZDEwNi00ZGRmLWE0NmMtY2RhOWY5ZGNlYWY1LyIsImlhdCI6MTY5NTg5NDc0MiwibmJmIjoxNjk1ODk0NzQyLCJleHAiOjE2OTU4OTg2NDIsImFpbyI6IkUyRmdZRkNRcmd6UWpJK3dlVlY3ZGNPdHE4RXJBQT09IiwiYXBwaWQiOiIyOGZjYjJkZC0xM2Y5LTRiMzEtYWRhNi00OTJhZGMyM2Q3YmMiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC84YTg2NTMyMS1kMTA2LTRkZGYtYTQ2Yy1jZGE5ZjlkY2VhZjUvIiwib2lkIjoiNzdlY2FmNTMtZWNjOS00ZTFiLWIzZTQtOWUyZDcwODJlYTliIiwicmgiOiIwLkFRd0FJVk9HaWdiUjMwMmtiTTJwLWR6cTlmMi03djNRN2dKTm9Ydnc5Rm1Ea1Y2V0FBQS4iLCJyb2xlcyI6WyJBcGkuY29uc3VtZSJdLCJzdWIiOiI3N2VjYWY1My1lY2M5LTRlMWItYjNlNC05ZTJkNzA4MmVhOWIiLCJ0aWQiOiI4YTg2NTMyMS1kMTA2LTRkZGYtYTQ2Yy1jZGE5ZjlkY2VhZjUiLCJ1dGkiOiJ3c2lPbm5WQmprYVFvalg0eUJkY0FBIiwidmVyIjoiMS4wIn0.A0GG_OewmkFNmPu4w-vlD38Jt1g5S8lchsuskIY07OjhSwwSpx5nFbulWwBW_DtJj-IzHNGbjHQW1EqOCesBeQAaA8300S3zFWEfTCU57b-CvuBfrZtXQlxV3y7-kAZOtl-XJfUyVh12MlVGNh8P7rbFP1bunXSTjBdBhxaCBQMA5eHeUcmUgnp4ie60bEzRQldFF-dm9DKIhHsE8_UFQoP4I6atQSIHzRrucf9-bpAwLx11SfLtBlPTsIxUoE4w2MyzQzJS71ZZOOZS_tlzIm-4xXfZbIZghg0q3sNXXxSinhxbZ0ycij_abx8sUDT1YhqEfTrg4P8S6g_FvSLXvw";
    }
}
