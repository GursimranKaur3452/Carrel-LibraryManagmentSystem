﻿Imports System.Windows.Threading
Imports Newtonsoft.Json

Public Class MemberAccount
    Dim _dashBoard As DashBoard = Application.Current.Windows(0)
    WithEvents _sendImage As DispatcherTimer
    Dim _camera As Camera
    Public sub New ()

        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
    End Sub
    Public Async sub GetData(uid As String)
        dim data as ArrayList = await MemberService.GetMember(UID)
        BorrowedList.ItemsSource = new List(Of Book) (Await BookService.GetBooksBorrowed(UID))
        LblUID.Content = UID
        LblName.Content = data(0) + "" + data(1)
        LblPhone.Content = data(2)
        LblDepartment.Content = data(3)
        LblSemester.Content = data(4)
        _dashBoard.MemberPopup.Content = me
        _dashBoard.MemberPopupDialog.IsOpen = True
    End sub
    Public Sub StartCameraAndTimer()
        _camera = New Camera
        _camera.StartCamera()
        _sendImage = New DispatcherTimer With {
            .Interval = New TimeSpan(0, 0, 0.2)
            }
        _sendImage.Start()
    End Sub

    Public sub StopCameraAndTimer() 
         _camera.StopCamera()
        _sendImage.Stop()
    End sub

    Public Sub SendImage_Tick(sender As Object, e As EventArgs) Handles _sendImage.Tick
        Dim qrDecoder As New QRDecoder
        Dim jsonString = ""
        jsonString = qrDecoder.ScanQR(_camera.frame)
        Try
            If jsonString <> "" Then
                StopCameraAndTimer
                QrScanned(jsonString)
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Async Sub QrScanned(jsonString As String)
        My.Computer.Audio.Play(My.Resources.ScannerBeep, AudioPlayMode.Background)
        Try
            If jsonString.Contains("UID") Then
                if Await CheckBookInList 'Checks and returns book if found
                     updateList("MEM-0001")
                ElseIf await BookService.Borrowed("BOOK-2","MEM-0001") 
                    updateList("MEM-0001")
                Else 
                    MsgBox("Failed to Update database")
                End If
            else
                MsgBox("Please scan a book to add or return")
            End If
        Catch ex As Exception
            msgbox(ex.ToString())
        End Try

        
    End Sub

    Private async Sub UpdateList(memberId As String)
        BorrowedList.ItemsSource =new List(Of Book) (await BookService.GetBooksBorrowed("MEM-0001"))
        StartCameraAndTimer
    End Sub

    Private Async Function CheckBookInList() As Task(Of Boolean)
        dim flag = False
        for each item in BorrowedList.Items
            If item.BookID.ToString().Contains("BOOK-2")
               Await BookService.Returned("BOOK-2")
                FLAG =1
            End If
        Next
       Return FLAG
    End Function

    Private Sub MemberAccount_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        StartCameraAndTimer
    End Sub

End Class
Class Book
    Property Sl
    Property BookId
    Property Title
    Property BorrowedOn
End Class
