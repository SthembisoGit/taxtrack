using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TaxTrack.Tests;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task CompanyList_ReturnsOnlyAccessibleCompanies()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var ownerToken = await RegisterAndLoginAsync(client, "owner-company-list@taxtrack.test");
        var outsiderToken = await RegisterAndLoginAsync(client, "outsider-company-list@taxtrack.test");

        await CreateCompanyAsync(client, ownerToken, "REG-COMPANY-001");
        await CreateCompanyAsync(client, ownerToken, "REG-COMPANY-002");
        await CreateCompanyAsync(client, outsiderToken, "REG-COMPANY-003");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/company");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var companies = json.RootElement.EnumerateArray().ToArray();

        Assert.Equal(2, companies.Length);
        Assert.Contains(companies, x => x.GetProperty("registrationNumber").GetString() == "REG-COMPANY-001");
        Assert.Contains(companies, x => x.GetProperty("registrationNumber").GetString() == "REG-COMPANY-002");
        Assert.DoesNotContain(companies, x => x.GetProperty("registrationNumber").GetString() == "REG-COMPANY-003");
    }

    [Fact]
    public async Task Upload_WithoutIdempotencyKey_ReturnsBadRequest()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAndLoginAsync(client, "owner-upload-header@taxtrack.test");
        var companyId = await CreateCompanyAsync(client, token, "REG-UPLOAD-001");

        using var content = BuildTransactionsFormContent(companyId, "REG-UPLOAD-001");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/financial/upload");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = content;

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Idempotency-Key header is required.", body);
    }

    [Fact]
    public async Task Upload_IdempotencyKey_ReusesExistingJob()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAndLoginAsync(client, "owner-upload-idem@taxtrack.test");
        var companyId = await CreateCompanyAsync(client, token, "REG-UPLOAD-002");

        var firstUploadId = await UploadTransactionsAsync(client, token, companyId, "REG-UPLOAD-002", "idem-upload-001");
        var secondUploadId = await UploadTransactionsAsync(client, token, companyId, "REG-UPLOAD-002", "idem-upload-001");

        Assert.Equal(firstUploadId, secondUploadId);
    }

    [Fact]
    public async Task Analyze_IdempotencyKey_ReusesExistingAnalysisJob()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAndLoginAsync(client, "owner-analyze-idem@taxtrack.test");
        var companyId = await CreateCompanyAsync(client, token, "REG-ANALYZE-001");

        var firstAnalysisId = await AnalyzeAsync(client, token, companyId, "idem-analyze-001");
        var secondAnalysisId = await AnalyzeAsync(client, token, companyId, "idem-analyze-001");

        Assert.Equal(firstAnalysisId, secondAnalysisId);
    }

    [Fact]
    public async Task NonMemberUser_CannotAccessCompanyRisk()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var ownerToken = await RegisterAndLoginAsync(client, "owner-access@taxtrack.test");
        var outsiderToken = await RegisterAndLoginAsync(client, "outsider-access@taxtrack.test");
        var companyId = await CreateCompanyAsync(client, ownerToken, "REG-ACCESS-001");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/risk/{companyId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ReplayIsRejected()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var tokens = await RegisterAndLoginTokensAsync(client, "owner-refresh@taxtrack.test");

        using var firstRefreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = tokens.RefreshToken
        });
        Assert.Equal(HttpStatusCode.OK, firstRefreshResponse.StatusCode);

        using var firstRefreshJson = JsonDocument.Parse(await firstRefreshResponse.Content.ReadAsStringAsync());
        var rotatedRefreshToken = firstRefreshJson.RootElement.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(rotatedRefreshToken));
        Assert.NotEqual(tokens.RefreshToken, rotatedRefreshToken);

        using var replayResponse = await client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = tokens.RefreshToken
        });
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);
    }

    [Fact]
    public async Task PrivacyRequest_OwnerCanCreateAndRead()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var ownerToken = await RegisterAndLoginAsync(client, "owner-privacy@taxtrack.test");
        var outsiderToken = await RegisterAndLoginAsync(client, "outsider-privacy@taxtrack.test");
        var companyId = await CreateCompanyAsync(client, ownerToken, "REG-PRIVACY-001");
        var requestId = await CreatePrivacyRequestAsync(client, ownerToken, companyId, "Export");

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/privacy/data-requests/{requestId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        using var getResponse = await client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var outsiderGetRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/privacy/data-requests/{requestId}");
        outsiderGetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);

        using var outsiderGetResponse = await client.SendAsync(outsiderGetRequest);
        Assert.Equal(HttpStatusCode.NotFound, outsiderGetResponse.StatusCode);
    }

    [Fact]
    public async Task PrivacyRequest_NonMemberCannotCreateForCompany()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var ownerToken = await RegisterAndLoginAsync(client, "owner-privacy-access@taxtrack.test");
        var outsiderToken = await RegisterAndLoginAsync(client, "outsider-privacy-access@taxtrack.test");
        var companyId = await CreateCompanyAsync(client, ownerToken, "REG-PRIVACY-002");

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/privacy/data-requests");
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);
        createRequest.Content = JsonContent.Create(new
        {
            companyId,
            requestType = "Deletion",
            reason = "test-delete"
        });

        using var createResponse = await client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client, string email)
    {
        var tokens = await RegisterAndLoginTokensAsync(client, email);
        return tokens.AccessToken;
    }

    private static async Task<(string AccessToken, string RefreshToken)> RegisterAndLoginTokensAsync(HttpClient client, string email)
    {
        using (var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "StrongPass!1234",
            role = "Owner"
        }))
        {
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        }

        using var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "StrongPass!1234"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginJson.RootElement.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("accessToken missing from login response.");
        var refreshToken = loginJson.RootElement.GetProperty("refreshToken").GetString()
            ?? throw new InvalidOperationException("refreshToken missing from login response.");

        return (accessToken, refreshToken);
    }

    private static async Task<Guid> CreateCompanyAsync(HttpClient client, string token, string registrationNumber)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/company");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            name = "API Test Company",
            registrationNumber,
            industry = "Technology",
            taxReference = "1234567890"
        });

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> UploadTransactionsAsync(
        HttpClient client,
        string token,
        Guid companyId,
        string registrationNumber,
        string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/financial/upload");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = BuildTransactionsFormContent(companyId, registrationNumber);

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("uploadId").GetGuid();
    }

    private static async Task<Guid> AnalyzeAsync(HttpClient client, string token, Guid companyId, string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/risk/analyze");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = JsonContent.Create(new { companyId });

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("analysisId").GetGuid();
    }

    private static async Task<Guid> CreatePrivacyRequestAsync(
        HttpClient client,
        string token,
        Guid companyId,
        string requestType)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/privacy/data-requests");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            companyId,
            requestType,
            reason = "test-reason"
        });

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("requestId").GetGuid();
    }

    private static MultipartFormDataContent BuildTransactionsFormContent(Guid companyId, string registrationNumber)
    {
        var csv = string.Join('\n',
            "contract_version,source_record_id,company_registration_number,transaction_date,ledger_category,description,amount,currency,vat_amount,direction,source_system,tax_invoice_number,supplier_vat_number,tax_invoice_date,vat201_reference",
            $"v1,txn-api-001,{registrationNumber},2026-01-31,Revenue,Sales invoice,1000.00,ZAR,0.00,credit,Manual,,,,");

        var fileBytes = Encoding.UTF8.GetBytes(csv);
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

        var form = new MultipartFormDataContent();
        form.Add(new StringContent(companyId.ToString()), "CompanyId");
        form.Add(new StringContent("transactions"), "DatasetType");
        form.Add(fileContent, "File", "transactions.csv");
        return form;
    }
}
