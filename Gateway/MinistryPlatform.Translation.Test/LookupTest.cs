namespace MinistryPlatform.Translation.Test
{
    //[TestFixture]
    //[Category("IntegrationTests")]
    //public class LookupTest
    //{
    //    private const string USERNAME = "changeme";
    //    private const string PASSWORD = "testme";
    //    private const string EMAIL = "donotreply+testme@crossroads.net";

    //    private AuthenticationServiceImpl _fixture;
    //    private PlatformServiceClient _platformService;
    //    private LookupRepository _lookupRepository;
    //    private IConfigurationWrapper _configurationWrapper;
    //    private IMinistryPlatformService _ministryPlatformService;


    //    [SetUp]
    //    public void SetUp()
    //    {
    //        _configurationWrapper = new ConfigurationWrapper();
    //        _platformService = new PlatformServiceClient();
    //        _ministryPlatformService = new MinistryPlatformServiceImpl(_platformService, _configurationWrapper);
    //        _lookupRepository = new LookupRepository(_fixture, _configurationWrapper, _ministryPlatformService);
    //        _fixture = new AuthenticationServiceImpl(_platformService, _ministryPlatformService);
    //    }

    //    [Test]
    //    public void ShouldReturnAValidObjectWithUserIdAndEmailAddress()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        var contactId = _fixture.GetContactId(token);
    //        Assert.IsNotNull(contactId);
    //        var emails = _lookupRepository.EmailSearch(EMAIL, token);
    //        Assert.IsNotEmpty(emails);
    //    }

    //    [Test]
    //    public void ShouldReturnValidObjectForUpperCaseEmailAddress()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        var contactId = _fixture.GetContactId(token);
    //        Assert.IsNotNull(contactId);
    //        var emails = _lookupRepository.EmailSearch(EMAIL.ToUpper(), token);
    //        Assert.IsNotEmpty(emails);
    //    }

    //    [Test]
    //    public void ShouldBeEmpty()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        var contactId = _fixture.GetContactId(token);
    //        Assert.IsNotNull(contactId);
    //        var emails = _lookupRepository.EmailSearch("CRAP@CRAP.com", token);
    //        Assert.IsEmpty(emails);
    //    }

    //    [Test]
    //    public void ShouldFindListOfGenders()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        List<Dictionary<string, object>> genders = _lookupRepository.Genders(token);
    //        Assert.IsNotEmpty(genders);
    //        genders.ForEach(x => { Assert.IsInstanceOf<Dictionary<string, object>>(x); });
    //    }

    //    [Test]
    //    public void ShouldFindListOfMaritalStatus()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        List<Dictionary<string, object>> maritalStatus = _lookupRepository.MaritalStatus(token);
    //        Assert.IsNotEmpty(maritalStatus);
    //        maritalStatus.ForEach(x => { Assert.IsInstanceOf<Dictionary<string, object>>(x); });
    //    }

    //    [Test]
    //    public void ShouldFindListOfServiceProviders()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        List<Dictionary<string, object>> ServiceProviders = _lookupRepository.ServiceProviders(token);
    //        Assert.IsNotEmpty(ServiceProviders);
    //        ServiceProviders.ForEach(x => { Assert.IsInstanceOf<Dictionary<string, object>>(x); });
    //    }

    //    [Test]
    //    public void ShouldFindListOfStates()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        List<Dictionary<string, object>> States = _lookupRepository.States(token);
    //        Assert.IsNotEmpty(States);
    //        States.ForEach(x => { Assert.IsInstanceOf<Dictionary<string, object>>(x); });
    //    }

    //    [Test]
    //    public void ShouldFindListOfCountries()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        List<Dictionary<string, object>> Countries = _lookupRepository.Countries(token);
    //        Assert.IsNotEmpty(Countries);
    //        Countries.ForEach(x => { Assert.IsInstanceOf<Dictionary<string, object>>(x); });
    //    }

    //    [Test]
    //    public void ShouldFindListOfCrossroadsLocations()
    //    {
    //        var clifton = new Dictionary<string, object> { { "dp_RecordID", 11 }, { "dp_RecordName", "Uptown" } };
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);

    //        var token = authData["token"].ToString();
    //        var crossroadsLocations = _lookupRepository.CrossroadsLocations(token);
    //        Assert.IsNotEmpty(crossroadsLocations);

    //        Assert.Contains(clifton, crossroadsLocations);
    //        crossroadsLocations.ForEach(Assert.IsInstanceOf<Dictionary<string, object>>);
    //    }

    //    [Test]
    //    public void ShouldFindListOfMinistries()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        List<Dictionary<string, object>> ministriesList = _lookupRepository.Ministries(token);
    //        Assert.IsNotEmpty(ministriesList);
    //        ministriesList.ForEach(x => { Assert.IsInstanceOf<Dictionary<string, object>>(x); });
    //    }

    //    [Test]
    //    public void ShouldFindListOfChildCareLocations()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        var contactId = _fixture.GetContactId(token);
    //        Assert.IsNotNull(contactId);
    //        var childcarelocations = _lookupRepository.ChildcareLocations(token);
    //        Assert.IsNotEmpty(childcarelocations);
    //    }

    //    [Test]
    //    public void ShouldFindGroups()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        var contactId = _fixture.GetContactId(token);
    //        Assert.IsNotNull(contactId);

    //        var groups = _lookupRepository.GroupsByCongregationAndMinistry(token,"1","11");
    //        Assert.IsNotEmpty(groups);
    //    }

    //    [Test]
    //    public void ShouldFindChildcareTimes()
    //    {
    //        var authData = AuthenticationRepository.authenticate(USERNAME, PASSWORD);
    //        Assert.IsNotNull(authData);
    //        var token = authData["token"].ToString();
    //        var contactId = _fixture.GetContactId(token);
    //        Assert.IsNotNull(contactId);

    //        var times = _lookupRepository.ChildcareTimesByCongregation(token, "1");
    //        Assert.IsNotEmpty(times);
    //    }
    //}
}
