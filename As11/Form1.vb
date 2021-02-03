'------------------------------------------------------------
'-                File Name : Form1.frm                     - 
'-                Part of Project: Assign11                  -
'------------------------------------------------------------
'-                Written By: Alex Buckstiegel              -
'-                Written On: 4-20-20         -
'------------------------------------------------------------
'- File Purpose:                                            -
'- This file contains the server side of the application 
'------------------------------------------------------------
'- Program Purpose:                                         -
'-                                                          -
'- This program plays a game of mancala, but through the internet!    -
'------------------------------------------------------------
'- Global Variable Dictionary (alphabetically):             -
'- Server - TcpListener for the server
'- aConn - a connection
'- NetStream - Stream of network info
'- NetWriter - Writes the NetStream
'- NetReader - Reads the NetStream
'- GetDataThread - Thread that gets the data
'- currentPlayer - Boolean variables determining the player
'- btnLast - Contains a copy of the lsat button clicked
'- changedPlayer - Boolean if the player hanged
'------------------------------------------------------------
Imports System.Threading
Imports System.Net.Sockets
Imports System.IO
Imports System.Text
Imports System.ComponentModel

Public Class Form1
    Dim Server As TcpListener
    Dim aConn As Socket
    Dim NetStream As NetworkStream
    Dim NetWriter As BinaryWriter
    Dim NetReader As BinaryReader
    Dim GetDataThread As Thread
    Dim currentPlayer As Boolean
    Const P1 = True
    Const P2 = False
    Dim btnLast As Button
    Dim changedPlayer = True

    '------------------------------------------------------------
    '-                Subprogram Name: Form1_Load            -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- Form load, sets the current Player to Player 1, and then
    '- sets CheckForIllegalCrossThreadCalls to false
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '- sender – Identifies which particular control raised the  –
    '-          click event                                     - 
    '- e – Holds the EventArgs object sent to the routine       -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        currentPlayer = P1
        CheckForIllegalCrossThreadCalls = False
    End Sub

    '------------------------------------------------------------
    '-                Subprogram Name: btnStartServer_Click     -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '- Runs the startup process of the listening process and 
    '- calls boardsetup
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '- sender – Identifies which particular control raised the  –
    '-          click event                                     - 
    '- e – Holds the EventArgs object sent to the routine       -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------
    Private Sub btnStartServer_Click(sender As Object, e As EventArgs) Handles btnStartServer.Click
        Try
            txtLog.Text &= "Starting Server..." & vbCrLf

            Server = New TcpListener(Net.IPAddress.Parse("127.0.0.1"), CInt(txtServerPort1.Text))
            Server.Start()
            btnStopServer.Enabled = True
            btnStartServer.Enabled = False
            txtLog.Text &= "Listening for client connection..." & vbCrLf
            Application.DoEvents()
            aConn = Server.AcceptSocket
            txtLog.Text &= "..client connection accepted"

            NetStream = New NetworkStream(aConn)

            NetWriter = New BinaryWriter(NetStream)
            NetReader = New BinaryReader(NetStream)

            txtLog.Text &= "Network Stream and Reader/writer objects created" & vbCrLf

            txtLog.Text &= "Preparing thread to watch for data..." & vbCrLf
            GetDataThread = New Thread(AddressOf GetDataFromClient)
            GetDataThread.Start()


        Catch IOEx As IOException
            txtLog.Text &= "Error Setting up Server -- Closing" & vbCrLf

        Catch SoecketEx As SocketException
            txtLog.Text &= "Server already exists -- just restarting listening" & vbCrLf


        Catch ex As Exception
            txtLog.Text &= ex.ToString
        End Try
        BoardSetup()
    End Sub

    '------------------------------------------------------------
    '-                Subprogram Name: GetDataFromClient            -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '- Gets the data from the client, runs a constant loop checking
    '- for new data, and parsing for new data
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- TheData - String of the Data
    '------------------------------------------------------------
    Sub GetDataFromClient()
        Dim TheData As String

        txtLog.Text &= "Data watching thread active" & vbCrLf

        Try
            Do
                CheckForWin()
                TheData = NetReader.ReadString
                txtLog.Text &= TheData
                ParseTheData(TheData)
            Loop While (TheData <> "~~END~~") And aConn.Connected
            StopListening()

        Catch IOEx As IOException
            txtLog.Text &= "Closing connection with client..." & vbCrLf
            StopListening()

        Catch ex As Exception

        End Try
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: ParseTheData             -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- Parses the data sent, and updates the board appropriately
    '- also swaps the player if necessary
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '- TheData - String of the the data
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- inputtedData()  - Array of the split of TheData
    '------------------------------------------------------------
    Sub ParseTheData(TheData As String)
        Dim inputtedData() = TheData.Split("-")
        For int As Integer = 0 To 11
            For Each button In pnlGameBoard.Controls
                If button.tag = int Then
                    button.text = inputtedData(int + 1)
                End If
            Next
        Next
        'TEMP
        If inputtedData(13) = "T" Then
            ChangePlayer()
            ChangeEnabled()
        End If
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: btnStopServer_Click            -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- callss the StopListening(), which stops listening for data
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '- sender – Identifies which particular control raised the  –
    '-          click event                                     - 
    '- e – Holds the EventArgs object sent to the routine       -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------
    Private Sub btnStopServer_Click(sender As Object, e As EventArgs) Handles btnStopServer.Click
        StopListening()
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: StopListening            -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- Shamelessly stolen from class example, but why reinvent 
    '- the wheel?
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------
    Sub StopListening()
        btnStartServer.Enabled = True
        btnStopServer.Enabled = False

        txtLog.Text &= "Attempting to close connection to client" & vbCrLf

        Try
            NetWriter.Write("~~END~~")
        Catch ex As Exception

        End Try
        Try
            NetWriter.Close()
            NetReader.Close()
            NetStream.Close()
            Server.Stop()
            NetWriter = Nothing
            NetReader = Nothing
            Server = Nothing
            Try
                GetDataThread.Abort()
            Catch ex As Exception

            End Try

        Catch ex As Exception

        Finally
            txtLog.Text &= "Server had been stopped" & vbCrLf
        End Try

    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: BoardSetup()            -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- Sets the board to the default values
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '- sender – Identifies which particular control raised the  –
    '-          click event                                     - 
    '- e – Holds the EventArgs object sent to the routine       -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------
    Sub BoardSetup()
        btnBottom1.Text = "5"
        btnBottom2.Text = "5"
        btnBottom3.Text = "5"
        btnBottom4.Text = "5"
        btnBottom5.Text = "5"

        btnTop1.Text = "5"
        btnTop2.Text = "5"
        btnTop3.Text = "5"
        btnTop4.Text = "5"
        btnTop5.Text = "5"

        btnLeftBig.Text = "0"
        btnRightBig.Text = "0"

        If radPlayer1.Checked Then
            btnTop1.Enabled = True
            btnTop2.Enabled = True
            btnTop3.Enabled = True
            btnTop4.Enabled = True
            btnTop5.Enabled = True
        Else
            btnBottom1.Enabled = True
            btnBottom2.Enabled = True
            btnBottom3.Enabled = True
            btnBottom4.Enabled = True
            btnBottom5.Enabled = True
        End If
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: btnTopClick            -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- Runs when any of the top board buttons are pushed. Distributes
    '- the points to the other buttons, determines if the player
    '- can go again, and checks for a win. 
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '- sender – Identifies which particular control raised the  –
    '-          click event                                     - 
    '- e – Holds the EventArgs object sent to the routine       -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- butttonValue - senders value
    '- buttonPosition - senders position
    '- total - buttonPosition + buttonValue used for determing 
    '- extra points
    '- extra - total-11 for extra points that need to be looped around
    '------------------------------------------------------------
    Private Sub btnTopClick(sender As Object, e As EventArgs) Handles btnTop1.Click, btnTop2.Click, btnTop3.Click, btnTop4.Click, btnTop5.Click
        Dim buttonValue As Integer = CInt(sender.text)
        Dim buttonPostition As Integer = CInt(sender.tag)
        If buttonPostition + buttonValue < 12 Then
            For position As Integer = buttonPostition To buttonValue + buttonPostition
                For Each button In pnlGameBoard.Controls
                    If button.tag = position Then
                        button.text = CStr(CInt(button.text) + 1)
                        btnLast = button
                    End If
                Next
            Next
        Else
            Dim total = buttonPostition + buttonValue
            Dim extra = total - 11
            For position As Integer = buttonPostition To 11
                For Each button In pnlGameBoard.Controls
                    If button.tag = position Then
                        button.text = CStr(CInt(button.text) + 1)
                        btnLast = button
                    End If
                Next
            Next
            For position As Integer = 0 To extra - 1
                For Each button In pnlGameBoard.Controls
                    If button.tag = position Then
                        button.text = CStr(CInt(button.text) + 1)
                        btnLast = button
                    End If
                Next
            Next
        End If
        If btnLast.Name = btnLeftBig.Name Or btnLast.Name = btnRightBig.Name Then
            lblWinner.Text = "Go again!"
            changedPlayer = False
        Else
            ChangePlayer()
            ChangeEnabled()
            changedPlayer = True
        End If
        CheckForWin()


        sender.text = "0"
        GeneratePacket()
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: GeneratePacket
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '- Generates the string packet to send to the other player
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- StringToSend - StringBuuilder of eventual final string   -
    '- strFinal - actual final string that is send to NetWriter
    '------------------------------------------------------------
    Sub GeneratePacket()
        Dim StringToSend As New StringBuilder
        'String Formatting:
        'P - The first letter will be P when there is a message sent
        StringToSend.Append("P")
        '1 or 2 - The next letter will be which player made the change. Since this is the server's code, and the Server is player 1, it will be hardcoded to be 1
        StringToSend.Append("1")
        'Next will go through all of the controls in the game board in order
        Dim counter = 0
        For position As Integer = 0 To 11
            For Each button In pnlGameBoard.Controls
                If button.tag = position Then
                    StringToSend.Append("-")
                    StringToSend.Append(button.text)
                    counter += 1

                End If
            Next
        Next
        StringToSend.Append("-")
        If changedPlayer Then
            StringToSend.Append("T")
        Else
            StringToSend.Append("F")
        End If
        StringToSend.Append("-")
        StringToSend.AppendLine("")
        Dim strFinal = StringToSend.ToString
        txtLog.Text &= strFinal
        NetWriter.Write(strFinal)
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: ChangePlayer            -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- Changes the current player
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------
    Sub ChangePlayer()
        If currentPlayer = P1 Then
            currentPlayer = P2
        Else
            currentPlayer = P1
        End If
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: ChangeEnabled
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '-                                                          -
    '- Toggles whether or not the buttons can be pressed
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------
    Sub ChangeEnabled()
        If currentPlayer = P1 Then
            btnTop1.Enabled = True
            btnTop2.Enabled = True
            btnTop3.Enabled = True
            btnTop4.Enabled = True
            btnTop5.Enabled = True
        Else
            btnTop1.Enabled = False
            btnTop2.Enabled = False
            btnTop3.Enabled = False
            btnTop4.Enabled = False
            btnTop5.Enabled = False
        End If
    End Sub
    '------------------------------------------------------------
    '-                Subprogram Name: CheckForWin              -
    '------------------------------------------------------------
    '-                Written By: Alex Buckstiegel              -
    '-                Written On: 4-20-20
    '------------------------------------------------------------
    '- Subprogram Purpose:                                      -
    '- Checks for the win conditions, but for some reason it doesn't
    '- work all that well and I cannot for the life of me figure out
    '- why
    '------------------------------------------------------------
    '- Parameter Dictionary (in parameter order):               -
    '- sender – Identifies which particular control raised the  –
    '-          click event                                     - 
    '- e – Holds the EventArgs object sent to the routine       -
    '------------------------------------------------------------
    '- Local Variable Dictionary (alphabetically):              -
    '- (None)                                                   -
    '------------------------------------------------------------
    Sub CheckForWin()
        If btnBottom1.Text = "0" And btnBottom2.Text = "0" And btnBottom3.Text = "0" And btnBottom4.Text = "0" And btnBottom5.Text = "0" Then
            For Each button In pnlGameBoard.Controls
                button.enabled = False
                lblWinner.Text = "Player 2 Wins!"
            Next
        End If
        If btnTop1.Text = "0" And btnTop2.Text = "0" And btnTop3.Text = "0" And btnTop4.Text = "0" And btnTop5.Text = "0" Then
            For Each button In pnlGameBoard.Controls
                button.enabled = False
                lblWinner.Text = "Player 1 Wins!"
            Next
        End If
    End Sub
    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        StopListening()
    End Sub
End Class
