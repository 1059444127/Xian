#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Desktop.Tables;
using ClearCanvas.Enterprise.Common.Admin.UserAdmin;

namespace ClearCanvas.Enterprise.Desktop
{
    public class UserTable : Table<UserSummary>
    {
        public UserTable()
        {
            Columns.Add(new TableColumn<UserSummary, string>(SR.ColumnUserId,
                                                             user => user.UserName,
                                                             0.5f));

            Columns.Add(new TableColumn<UserSummary, string>(SR.ColumnUserName,
                                                             user => user.DisplayName,
                                                             1.0f));

            Columns.Add(new DateTimeTableColumn<UserSummary>("Created On",
                                                             user => user.CreationTime,
                                                             0.75f));

            Columns.Add(new TableColumn<UserSummary, bool>("Enabled",
                                                           user => user.Enabled,
                                                           0.25f));

            Columns.Add(new DateTimeTableColumn<UserSummary>("Valid From",
                                                             user => user.ValidFrom,
                                                             0.75f));

            Columns.Add(new DateTimeTableColumn<UserSummary>("Valid Until",
                                                             user => user.ValidUntil,
                                                             0.75f));

            Columns.Add(new DateTimeTableColumn<UserSummary>("Password Expiry",
                                                             user => user.PasswordExpiry,
                                                             0.75f));

            Columns.Add(new DateTimeTableColumn<UserSummary>("Last Login Time",
                                                             user => user.LastLoginTime,
                                                             0.75f));
        }
    }
}
