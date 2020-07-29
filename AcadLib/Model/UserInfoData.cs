namespace AcadLib
{
    using System.Collections.Generic;
    using MongoDblib.UsersData.Data;

    public class UserInfoData
    {
        public string FioAD { get; set; }

        public UserData UserData { get; set; }

        public List<string> UserGroupsAd { get; set; }
    }
}