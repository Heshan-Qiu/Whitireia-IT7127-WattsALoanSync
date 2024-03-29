﻿Imports Microsoft.SqlServer.Management.Common
Imports Microsoft.SqlServer.Replication

Public Class FormCustomer
    Private Sub CustomersBindingNavigatorSaveItem_Click(sender As Object, e As EventArgs) Handles CustomersBindingNavigatorSaveItem.Click
        Me.Validate()
        Me.CustomersBindingSource.EndEdit()
        Me.TableAdapterManager.UpdateAll(Me.WattsALoanSubDataSet)

    End Sub

    Private Sub FormCustomer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'TODO: This line of code loads data into the 'WattsALoanSubDataSet.Customers' table. You can move, or remove it, as needed.
        Me.CustomersTableAdapter.Fill(Me.WattsALoanSubDataSet.Customers)

    End Sub

    Private Sub ButtonSync_Click(sender As Object, e As EventArgs) Handles ButtonSync.Click
        Dim subscriberName As String = "PE201-15\MSSQLSVR" 'Machine name of subcriber 
        Dim publisherName As String = "PE201-15\MSSQLSVR" 'Machine name of publisher 
        Dim publicationName As String = "WattsALoanPub" 'Name given to publication when publication first created using wizard
        Dim subscriptionDbName As String = "WattsALoanSub" 'Name of subscription database
        Dim publicationDbName As String = "WattsALoanPub" 'Name of publication database
        Dim hostname As String = "PCSUPPORT\RepListen" 'Machine name and username of account used by replication agents 
        Dim webSyncUrl As String = "https://" + "pe201-15.pcsupport.ac.nz" + "/SQLReplication/replisapi.dll" 'Issued-to name of self signed certificate + folder of replisapi.dll

        ' Create a connection to the Subscriber.
        Dim conn As ServerConnection = New ServerConnection(subscriberName)

        Dim subscription As MergePullSubscription
        Dim agent As MergeSynchronizationAgent

        Try
            ' Connect to the Subscriber.
            conn.Connect()

            ' Define the pull subscription.
            subscription = New MergePullSubscription()
            subscription.ConnectionContext = conn
            subscription.DatabaseName = subscriptionDbName
            subscription.PublisherName = publisherName
            subscription.PublicationDBName = publicationDbName
            subscription.PublicationName = publicationName

            ' If the pull subscription exists, then start the synchronization.
            If subscription.LoadProperties() Then
                ' Get the agent for the subscription.
                agent = subscription.SynchronizationAgent

                ' Check that we have enough metadata to start the agent.
                If agent.PublisherSecurityMode = Nothing Then
                    ' Set the required properties that could not be returned
                    ' from the MSsubscription_properties table. 
                    agent.PublisherSecurityMode = SecurityMode.Integrated
                    agent.Distributor = "PE201-15\MSSQLSVR" 'Machine name where distributor located - yours will begin with PE203 or PE204
                    agent.DistributorSecurityMode = SecurityMode.Integrated
                    agent.HostName = hostname

                    ' Set optional Web synchronization properties.
                    agent.UseWebSynchronization = True
                    agent.InternetUrl = webSyncUrl
                    agent.InternetSecurityMode = SecurityMode.Standard
                    agent.InternetLogin = "PCSUPPORT\RepListen" 'Machine name and username of account used by replication agents
                    agent.InternetPassword = "" 'password not required - integrated security used instead
                End If

                ' Enable agent logging to the console.
                agent.OutputVerboseLevel = 1
                agent.Output = ""

                ' Synchronously start the Merge Agent for the subscription.
                agent.Synchronize()
            Else
                ' Do something here if the pull subscription does not exist.
                Throw New ApplicationException(String.Format(
                 "A subscription to '{0}' does not exist on {1}",
                 publicationName, subscriberName))
            End If
        Catch ex As Exception
            ' Implement appropriate error handling here.
            Throw New ApplicationException("The subscription could not be " +
             "synchronized. Verify that the subscription has " +
             "been defined correctly.", ex)
        Finally
            conn.Disconnect()
            MsgBox("Synchronise Success!!")
        End Try
    End Sub

    Private Sub ButtonUpdate_Click(sender As Object, e As EventArgs) Handles ButtonUpdate.Click
        Me.QueriesTableAdapter.UpdateCustomer(CustomerIDTextBox.Text, DateCreatedDateTimePicker.Value, FullNameTextBox.Text, BillingAddressTextBox.Text, BillingCityTextBox.Text, BillingStateTextBox.Text, BillingZIPCideTextBox.Text, EmailAddressTextBox.Text)
        Dim result As Integer = MessageBox.Show("Update Success! Would you like to synchronise now?", "Information", MessageBoxButtons.YesNo)
        If result = DialogResult.Yes Then
            ButtonSync.PerformClick()
        End If
    End Sub
End Class
