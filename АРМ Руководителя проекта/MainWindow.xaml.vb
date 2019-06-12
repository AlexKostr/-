Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Xaml

'Imports System.Data.SqlClient

Class MainWindow
    Public refinans As Double
    Public lot_p(2)()
    Private ReadOnly connectionString As String = GetConnectionString()
    Private adapter As System.Data.OleDb.OleDbDataAdapter
    Private DG1Table As DataTable
    Private DG2Table As DataTable
    Private DG3Table As DataTable
    Private Месяц_Показ_flg As Boolean = True
    Private КС_Показ_flg As Boolean = True
    Private Инит_flg As Boolean = False


    Private Sub Form1_Loaded(sender As Object, e As RoutedEventArgs) Handles Form1.Loaded
        Dim sql As String
        Dim i, j As Long
        Form1.Visibility = Visibility.Hidden
        Form1.TaskbarItemInfo.ProgressValue = 0
        Form1.TaskbarItemInfo.Description = "Загрузка начальных данных"
        Form1.TaskbarItemInfo.ProgressState = Shell.TaskbarItemProgressState.Normal
        UserName.Text = "Пользователь " & Environ("USERNAME")
        Form1.TaskbarItemInfo.ProgressValue = 0.1
        sql = "SELECT ПРП.User, ПРП.[all], ""Лот "" & [Лоты]![Номер лота] & "" Контракт № "" & [Номер договора] & "" от "" & [Дата договора] AS Выражение1, ПРП.[лот] FROM Лоты INNER JOIN ПРП ON Лоты.Код = ПРП.[лот] WHERE (((ПРП.User)=" + Chr(34) & Environ("USERNAME") & Chr(34) + "));"
        Dim rez1 As Object = ConnectToData(connectionString, sql) '"SELECT * FROM ПРП WHERE ПРП.User=" + Chr(34) & Environ("USERNAME") & Chr(34))
        If (UBound(rez1) - LBound(rez1)) < 1 Then
            MsgBox("Пользователь " & Environ("USERNAME") & " не имеет права работать с данным файлом. Прошу сообщить руководителю департамента управления проектом об этом для включения в список.", MsgBoxStyle.Critical, "Ошибка!!!")
            Form1.Close()
            Exit Sub
        End If

        ReDim lot_p(0)(UBound(rez1) - LBound(rez1) + 10)
        ReDim lot_p(1)(UBound(rez1) - LBound(rez1) + 10)
        Lot.Items.Clear()
        Lot.Items.IsLiveSorting = True
        For i = LBound(rez1) To UBound(rez1)
            j = Lot.Items.Add(rez1(i)(2))
            lot_p(0)(j) = rez1(i)(3)
            lot_p(1)(j) = rez1(i)(2)
        Next i
        Form1.TaskbarItemInfo.ProgressValue = 0.2
        Dim rez As Object = ConnectToData(connectionString, "Select Настройки.Значение FROM Настройки WHERE (((Настройки.Название)=""Ставка рефенансирования""));")
        Form1.TaskbarItemInfo.ProgressValue = 0.4
        refinans = CDbl(rez(0)(0))
        Refinans_text.Text = "Ставка рефинансирования ЦБ " & FormatPercent(refinans)
        Form1.TaskbarItemInfo.ProgressValue = 0.6

        Form1.TaskbarItemInfo.ProgressValue = 1
        Form1.TaskbarItemInfo.Description = ""
        TipV.Text = "График / РКЦ"
        Инит_flg = True
        Form1.Visibility = Visibility.Visible

        Form1SizeChanged(True, True)

        Lot.SelectedIndex = 0 ' Вызывает Lot_SelectionChanged и соответственно ReadDBtoDG

        Form1.TaskbarItemInfo.ProgressState = Shell.TaskbarItemProgressState.None
    End Sub

    Private Sub Form1_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Form1.SizeChanged
        Form1SizeChanged(e.WidthChanged, e.HeightChanged)
    End Sub
    Private Sub Form1SizeChanged(w As Boolean, h As Boolean)
        If Инит_flg Then
            If w Then
                Dim l As Long = GridMW.ActualWidth 'Form1.ActualWidth - 25
                If l < 20 Then l = 30
                MMenu.Width = l
                DP.Width = l - 10
                DG1.Width = l - 12
                If Месяц_Показ_flg Then
                    DG2.Width = (l - 12) / 2
                End If
                If КС_Показ_flg Then
                    DG3.Width = (l - 12) / 2
                End If
                SB.Width = l
                StatusText.Width = l - 20 - UserName.ActualWidth - Refinans_text.ActualWidth
            End If
            If h Then
                Dim l As Long = GridMW.ActualHeight - MMenu.ActualHeight - SB.ActualHeight ' - 26 'Form1.ActualHeight - MMenu.ActualHeight - SB.ActualHeight - 26
                If l < 20 Then l = 30
                DP.Height = l
                If Месяц_Показ_flg And (GuidR()) Then
                    DG2.Height = l / 3
                Else
                    DG2.Height = 0
                End If
                If КС_Показ_flg And (GuidR()) Then
                    DG3.Height = l / 3
                Else
                    DG3.Height = 0
                End If

                DG1.Height = l - DG2.Height ' - 12
            End If
        End If
    End Sub

    Private Function ConnectToData(connectionString As String, SQLStr As String) As Object

        Using connection As IDbConnection = New System.Data.OleDb.OleDbConnection(connectionString)
            Dim nas_dat() As Object = {}
            Dim i As Long
            Dim j As Integer

            Dim Command As System.Data.OleDb.OleDbCommand = New System.Data.OleDb.OleDbCommand(SQLStr, connection)
            connection.Open()
            i = 0
            Dim reader As System.Data.OleDb.OleDbDataReader = Command.ExecuteReader()
            While (reader.Read())
                Dim nas_dat1(i) As Object
                For j = 0 To i - 1
                    nas_dat1(j) = nas_dat(j)
                Next j
                ReDim nas_dat(i)
                For j = 0 To i - 1
                    nas_dat(j) = nas_dat1(j)
                Next j
                Dim n(reader.VisibleFieldCount) As Object
                For j = 0 To reader.VisibleFieldCount - 1
                    n(j) = reader(j)
                Next j
                nas_dat(i) = n
                i += 1
            End While
            ConnectToData = nas_dat
            reader.Close()
            connection.Close()
        End Using
    End Function

    Private Function GetConnectionString() As String
        GetConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\ORP.mdb"
    End Function

    Private Sub ABoxItem_Click(sender As Object, e As RoutedEventArgs)
        Dim fr As AboutBox1 = New AboutBox1()
        fr.ShowDialog()
    End Sub

    Private Sub UpdateDB()
        Dim comandbuilder As OleDbCommandBuilder = New OleDbCommandBuilder(adapter)
        adapter.Update(DG1Table)
    End Sub

    Private Sub ReadDBtoDG1()
        If TipV.Text = "График / РКЦ" Then
            DG1.Columns.Clear()
            Dim sql As String = "SELECT * FROM РКЦ WHERE Лот=" & lot_p(0)(Lot.SelectedIndex()) & " order by Лот, №пп"
            DG1Table = New DataTable()
            Dim connection As OleDbConnection = Nothing
            Try
                connection = New OleDbConnection(connectionString)
                Dim Command As OleDbCommand = New OleDbCommand(sql, connection)
                adapter = New OleDbDataAdapter(Command)
                Command = New OleDbCommand("INSERT INTO РКЦ (Лот, №пп, №РКЦ, НазваниеРКЦ, Ед_изм, Цена, Сумма, Дата_начала, Дата_конец, Разделение, Этап, ПСД, Регион, Объект, Примечание, БезНДС, Тип ) " + "VALUES (" + "?, ?)", connection)

                '             Command.Parameters.Add("КодРКЦ", OleDbType.Guid, 16, "КодРКЦ")
                Command.Parameters.Add("№пп", OleDbType.Integer, 40, "№пп")
                Command.Parameters.Add("№РКЦ", OleDbType.WChar, 255, "№РКЦ")
                Command.Parameters.Add("НазваниеРКЦ", OleDbType.WChar, 0, "НазваниеРКЦ")
                Command.Parameters.Add("Ед_изм", OleDbType.WChar, 5, "Ед_изм")
                Command.Parameters.Add("кол_во", OleDbType.Double, 15, "кол_во")
                Command.Parameters.Add("Цена", OleDbType.Double, 15, "Цена")
                Command.Parameters.Add("Сумма", OleDbType.Double, 15, "Сумма")
                Command.Parameters.Add("Дата_начала", OleDbType.Date, 15, "Дата_начала")
                Command.Parameters.Add("Дата_конец", OleDbType.Date, 15, "Дата_конец")
                Command.Parameters.Add("Разделение", OleDbType.WChar, 255, "Разделение")
                Command.Parameters.Add("Этап", OleDbType.WChar, 255, "Этап")
                Command.Parameters.Add("ПСД", OleDbType.WChar, 255, "ПСД")
                Command.Parameters.Add("Регион", OleDbType.WChar, 255, "Регион")
                Command.Parameters.Add("Объект", OleDbType.WChar, 255, "Объект")
                Command.Parameters.Add("Примечание", OleDbType.WChar, 255, "Примечание")
                Command.Parameters.Add("БезНДС", OleDbType.Boolean, 1, "БезНДС")
                Command.Parameters.Add("Тип", OleDbType.WChar, 25, "Тип")

                adapter.InsertCommand = Command

                ''            // установка команды на добавление для вызова хранимой процедуры
                'adapter.InsertCommand = New OleDbCommand("sp_InsertPhone", connection)
                'adapter.InsertCommand.CommandType = CommandType.StoredProcedure
                'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@title", OleDbType.VarChar, 50, "Title"))
                'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@company", OleDbType.VarChar, 50, "Company"))
                'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@price", OleDbType.BigInt, 0, "Price"))
                'Dim parameter As OleDbParameter = adapter.InsertCommand.Parameters.Add("@Id", OleDbType.BigInt, 0, "Id")
                'parameter.Direction = ParameterDirection.Output

                connection.Open()
                adapter.Fill(DG1Table)
                DG1.ItemsSource = DG1Table.DefaultView
                Dim i As Integer
                For i = 0 To 18
                    Console.WriteLine("Поле " & i & " " & DG1Table.Columns.Item(i).ColumnName & " | " & DG1Table.Columns.Item(i).DataType.ToString)
                Next i
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            Finally
                If connection.ToString <> Nothing Then connection.Close()
            End Try

        End If

    End Sub
    Private Function GuidR() As Boolean
        On Error GoTo ErrorHandler
        Dim t = DG1.CurrentCell().Item(0)
        GuidR = True
        Exit Function
ErrorHandler:
        GuidR = False
    End Function
    Private Sub ReadDBtoDG2()

        If (TipV.Text = "График / РКЦ") Then
            DG2.Columns.Clear()
            If (GuidR()) Then
                Dim dvg = DG1.CurrentCell().Item(0)

                Dim sql As String = "SELECT РКЦ_пункты.* FROM РКЦ_пункты WHERE (((РКЦ_пункты.РКЦ)={" & dvg.ToString & "})) ORDER BY РКЦ_пункты.Дата_окон, РКЦ_пункты.Дата_нач;"
                'SELECT * FROM РКЦ WHERE Лот=" & lot_p(0)(Lot.SelectedIndex()) & " order by Лот, №пп"
                DG2Table = New DataTable()
                Dim connection As OleDbConnection = Nothing
                Try
                    connection = New OleDbConnection(connectionString)
                    Dim Command As OleDbCommand = New OleDbCommand(sql, connection)
                    adapter = New OleDbDataAdapter(Command)
                    Command = New OleDbCommand("INSERT INTO РКЦ_пункты (РКЦ, Дата_нач, Дата_окон, Объем, Деньги) " + "VALUES ({" & dvg.ToString & "} , ?, ?, ?, ? )", connection)

                    '             Command.Parameters.Add("КодРКЦ", OleDbType.Guid, 16, "КодРКЦ")
                    Command.Parameters.Add("Дата_нач", OleDbType.Date, 15, "Дата_нач")
                    Command.Parameters.Add("Дата_окон", OleDbType.Date, 15, "Дата_окон")
                    Command.Parameters.Add("Объем", OleDbType.Double, 15, "Объем")
                    Command.Parameters.Add("Деньги", OleDbType.Double, 15, "Деньги")

                    adapter.InsertCommand = Command

                    ''            // установка команды на добавление для вызова хранимой процедуры
                    'adapter.InsertCommand = New OleDbCommand("sp_InsertPhone", connection)
                    'adapter.InsertCommand.CommandType = CommandType.StoredProcedure
                    'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@title", OleDbType.VarChar, 50, "Title"))
                    'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@company", OleDbType.VarChar, 50, "Company"))
                    'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@price", OleDbType.BigInt, 0, "Price"))
                    'Dim parameter As OleDbParameter = adapter.InsertCommand.Parameters.Add("@Id", OleDbType.BigInt, 0, "Id")
                    'parameter.Direction = ParameterDirection.Output

                    connection.Open()
                    adapter.Fill(DG2Table)
                    DG2.ItemsSource = DG2Table.DefaultView
                    Dim i As Integer
                    For i = 0 To 5
                        Console.WriteLine("Поле " & i & " " & DG2Table.Columns.Item(i).ColumnName & " | " & DG2Table.Columns.Item(i).DataType.ToString)
                    Next i
                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                Finally
                    If connection.ToString <> Nothing Then connection.Close()
                End Try
            End If
        End If
        Form1SizeChanged(True, True)

    End Sub
    Private Sub ReadDBtoDG3()

        If (TipV.Text = "График / РКЦ") Then
            DG3.Columns.Clear()
            If (GuidR()) Then
                Dim dvg = DG1.CurrentCell().Item(0)

                Dim sql As String = "SELECT РКЦ_КС.* FROM РКЦ_КС WHERE (((РКЦ_КС.РКЦ)={" & dvg.ToString & "})) ORDER BY РКЦ_КС.Дата_закрытия;"
                'SELECT * FROM РКЦ WHERE Лот=" & lot_p(0)(Lot.SelectedIndex()) & " order by Лот, №пп"
                DG3Table = New DataTable()
                Dim connection As OleDbConnection = Nothing
                Try
                    connection = New OleDbConnection(connectionString)
                    Dim Command As OleDbCommand = New OleDbCommand(sql, connection)
                    adapter = New OleDbDataAdapter(Command)
                    Command = New OleDbCommand("INSERT INTO РКЦ_пункты (РКЦ, Дата_закрытия, Объем, Деньги, НомерКС) " + "VALUES ({" & dvg.ToString & "} , ?, ?, ?, ? )", connection)

                    '             Command.Parameters.Add("КодРКЦ", OleDbType.Guid, 16, "КодРКЦ")
                    Command.Parameters.Add("Дата_закрытия", OleDbType.Date, 15, "Дата закрытия")
                    Command.Parameters.Add("Объем", OleDbType.Double, 15, "Объем")
                    Command.Parameters.Add("Деньги", OleDbType.Double, 15, "Деньги")
                    Command.Parameters.Add("НомерКС", OleDbType.Guid, 15, "Номер КС")

                    adapter.InsertCommand = Command

                    ''            // установка команды на добавление для вызова хранимой процедуры
                    'adapter.InsertCommand = New OleDbCommand("sp_InsertPhone", connection)
                    'adapter.InsertCommand.CommandType = CommandType.StoredProcedure
                    'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@title", OleDbType.VarChar, 50, "Title"))
                    'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@company", OleDbType.VarChar, 50, "Company"))
                    'adapter.InsertCommand.Parameters.Add(New OleDbParameter("@price", OleDbType.BigInt, 0, "Price"))
                    'Dim parameter As OleDbParameter = adapter.InsertCommand.Parameters.Add("@Id", OleDbType.BigInt, 0, "Id")
                    'parameter.Direction = ParameterDirection.Output

                    connection.Open()
                    adapter.Fill(DG3Table)
                    DG3.ItemsSource = DG3Table.DefaultView
                    Dim i As Integer
                    For i = 0 To 5
                        Console.WriteLine("Поле " & i & " " & DG3Table.Columns.Item(i).ColumnName & " | " & DG3Table.Columns.Item(i).DataType.ToString)
                    Next i
                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                Finally
                    If connection.ToString <> Nothing Then connection.Close()
                End Try
            End If
        End If
        Form1SizeChanged(True, True)

    End Sub

    Private Sub TipV_DropDownClosed(sender As Object, e As EventArgs) Handles TipV.DropDownClosed
        '      Console.WriteLine("DropDownClosed " & TipV.Text)
        ReadDBtoDG1()
    End Sub

    Private Sub DG1_RowEditEnding(sender As Object, e As DataGridRowEditEndingEventArgs) Handles DG1.RowEditEnding
        UpdateDB()
    End Sub

    'Public Function Create(type As Type) As DataTemplate

    '    Dim stringReader As IO.StringReader = New IO.StringReader("<DataTemplate         xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">             <" & type.Name & " Text=""{Binding " + ShowColumn + @"}""/>          </DataTemplate>")
    '    Dim xmlReader As Xml.XmlReader = Xml.XmlReader.Create(stringReader)
    '    Create = xmlReader.Load(xmlReader)

    'End Function
    Private Sub DG1_AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs) Handles DG1.AutoGeneratingColumn
        Dim headername As String = e.Column.Header.ToString()
        'Dim datetempl As DataGridTemplateColumn = New DataGridTemplateColumn()
        'datetempl.CellTemplate = еее

        If (headername = "MiddleName") Then e.Cancel = True

        Select Case headername
            Case "КодРКЦ"
                e.Column.Visibility = Visibility.Hidden
            Case "Лот"
                e.Column.Visibility = Visibility.Hidden
            Case "№пп"
                e.Column.Header = "№ п/п"
            Case "№РКЦ"
                e.Column.Header = "№ РКЦ"
            Case "НазваниеРКЦ"
                e.Column.Header = "Название"
            Case "Ед_изм"
                e.Column.Header = "Ед.изм"
            Case "кол_во"
                e.Column.Header = "кол-во"
            Case "Цена"
                e.Column.Header = "Цена"
            Case "Сумма"
                e.Column.Header = "Сумма"
            Case "Дата_начала"
                e.Column.Header = "Дата начала"

            Case "Дата_конец"
                e.Column.Header = "Дата конец"
            Case "Разделение"
                e.Column.Header = "Разделение"
            Case "Этап"
                e.Column.Header = "Этап"
            Case "ПСД"
                e.Column.Header = "ПСД"
            Case "Регион"
                e.Column.Header = "Регион"
            Case "Объект"
                e.Column.Header = "Объект"
            Case "Примечание"
                e.Column.Header = "Примечание"
            Case "БезНДС"
                e.Column.Header = "Без НДС"
            Case "Тип"
                e.Column.Header = "Тип"
        End Select
    End Sub
    Private Sub DG2_AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs) Handles DG2.AutoGeneratingColumn
        Dim headername As String = e.Column.Header.ToString()
        'Dim datetempl As DataGridTemplateColumn = New DataGridTemplateColumn()
        'datetempl.CellTemplate = еее

        If (headername = "MiddleName") Then e.Cancel = True

        Select Case headername
            Case "кодПункта"
                e.Column.Visibility = Visibility.Hidden
            Case "РКЦ"
                e.Column.Visibility = Visibility.Hidden
            Case "Дата_нач"
                e.Column.Header = "Дата начала"
            Case "Дата_окон"
                e.Column.Header = "Дата окончания"
            Case "Объем"
                e.Column.Header = "Объём"
            Case "Деньги"
                e.Column.Header = "Деньги"
        End Select

    End Sub

    Private Sub DG3_AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs) Handles DG3.AutoGeneratingColumn
        Dim headername As String = e.Column.Header.ToString()
        'Dim datetempl As DataGridTemplateColumn = New DataGridTemplateColumn()
        'datetempl.CellTemplate = еее

        If (headername = "MiddleName") Then e.Cancel = True

        Select Case headername
            Case "кодПункта"
                e.Column.Visibility = Visibility.Hidden
            Case "РКЦ"
                e.Column.Visibility = Visibility.Hidden
            Case "Дата_закрытия"
                e.Column.Header = "Дата закрытия"
            Case "НомерКС"
                e.Column.Header = "Номер КС"
            Case "Объем"
                e.Column.Header = "Объём"
            Case "Деньги"
                e.Column.Header = "Деньги"
        End Select

    End Sub


    Private Sub Lot_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Lot.SelectionChanged
        ReadDBtoDG1()
    End Sub


    Private Sub Месяц_Показ_Click(sender As Object, e As RoutedEventArgs) Handles Месяц_Показ.Click
        Месяц_Показ_flg = Not Месяц_Показ_flg
        Form1SizeChanged(True, True)
    End Sub

    Private Sub DG1_CurrentCellChanged(sender As Object, e As EventArgs) Handles DG1.CurrentCellChanged
        ReadDBtoDG2()
        ReadDBtoDG3()
    End Sub

    Private Sub КС_Показ_Click(sender As Object, e As RoutedEventArgs) Handles КС_Показ.Click
        КС_Показ_flg = Not КС_Показ_flg
        Form1SizeChanged(True, True)
    End Sub
End Class
