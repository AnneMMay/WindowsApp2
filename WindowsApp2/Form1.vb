Imports System.Data.SqlClient

Imports System
Imports System.IO
Imports System.Xml

Imports <xmlns='http://www.six-interbank-clearing.com/de/pain.001.001.03.ch.02.xsd'>


Module Mod1
    Private Cn As SqlConnection = New SqlConnection("Server = 192.168.1.4;Database=Beetool;Integrated Security=SSPI")

    Sub createxml()

        Dim tabelle As DataTable
        ''tabelle = SQL("SELECT name FROM tbl_benutzerberechtigungen")
        'tabelle = SQL("Create Table t_banktestXML(id int, vorname varchar(255), nachname varchar(255), bic varchar(255), iban varchar(255), amount float);")
        'tabelle = SQL("Insert into t_banktestXML  values (2, 'Anne', 'Maier', 'BLKBCH22', 'CH2400769431135062047', 1548.50);")
        'tabelle = SQL("Delete from t_banktestXML where vorname = 'Klaas';")
        'tabelle = SQL("Update t_banktestXML SET iban='CH2400769431135062030' where vorname = 'Madeleine'")

        tabelle = SQL("Select * from t_banktestXML")
        Dim crdt As List(Of Creditor) = New List(Of Creditor)
        Dim i = 0
        Dim amount_total As Double = 0

        For Each row As DataRow In tabelle.Rows

            ' teststring = Convert.ToString(row.Item(0)) & " " & Convert.ToString(row.Item(1)) & vbCr
            crdt.Add(New Creditor(Convert.ToString(row.Item(1)), Convert.ToString(row.Item(2)), Convert.ToString(row.Item(3)), Convert.ToString(row.Item(4)), Convert.ToInt16(row.Item(0)), Convert.ToDouble(row.Item(5))))
            amount_total = amount_total + crdt(i).Amount
            i = i + 1
        Next


        Dim ISODateandtime = Format$(Now(), "yyyy-MM-ddThh:mm:ss")
        Dim ISODate = Format$(Now(), "yyyy-MM-dd")
        Dim doc As XElement =
             <Document xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.six-interbank-clearing.com/de/pain.001.001.03.ch.02.xsd  pain.001.001.03.ch.02.xsd">
                 <CstmrCdtTrfInitn>
                     <GrpHdr>
                         <MsgId><%= "msgid" & DateTime.Today %></MsgId>
                         <CreDtTm><%= ISODateandtime %></CreDtTm>
                         <NbOfTxs><%= i %></NbOfTxs>
                         <CtrlSum><%= amount_total %></CtrlSum>
                         <InitgPty>
                             <Nm>beeworx GmbH</Nm>
                         </InitgPty>
                     </GrpHdr>
                     <PmtInf>
                         <PmtInfId>PMTINF-01</PmtInfId>
                         <PmtMtd>TRF</PmtMtd>
                         <ReqdExctnDt><%= ISODate %></ReqdExctnDt>
                         <Dbtr>
                             <Nm>MUSTER AG</Nm>
                         </Dbtr>
                         <DbtrAcct>
                             <Id>
                                 <IBAN>CH1000769431136312001</IBAN>
                             </Id>
                         </DbtrAcct>
                         <DbtrAgt>
                             <FinInstnId>
                                 <BIC>BLKBCH22</BIC>
                             </FinInstnId>
                         </DbtrAgt>
                         <%=
                             From j In crdt
                             Select
                         <CdtTrfTxInf>
                             <PmtId>
                                 <InstrId>instID-01</InstrId>
                                 <EndToEndId><%= "Reference" & Today %></EndToEndId>
                             </PmtId>
                             <Amt>
                                 <InstdAmt Ccy="CHF"><%= j.Amount %></InstdAmt>
                             </Amt>
                             <CdtrAgt>
                                 <FinInstnId>
                                     <BIC><%= j.BIC %></BIC>
                                 </FinInstnId>
                             </CdtrAgt>
                             <Cdtr>
                                 <Nm><%= j.Vorname & " " & j.Nachname %></Nm>
                             </Cdtr>
                             <CdtrAcct>
                                 <Id>
                                     <IBAN><%= j.IBAN %></IBAN>
                                 </Id>
                             </CdtrAcct>
                         </CdtTrfTxInf>
                         %>
                     </PmtInf>
                 </CstmrCdtTrfInitn>
             </Document>

        Dim finaldoc As XDocument = New XDocument
        finaldoc.Add(doc)

        Dim dec As XDeclaration = New XDeclaration("1.0", "utf-8", "")
        finaldoc.Declaration = dec

        Console.WriteLine(finaldoc)
        Console.WriteLine(ISODate)
        finaldoc.Save("C:\Users\anne maier\Desktop\testXML\1203181.xml")
    End Sub

    Public Function SQL(ByVal SQLstring As String, Optional ByVal connection As Boolean = True, Optional ByVal silent_try As Boolean = False) As DataTable
        Dim table As New DataTable
        If SQLstring <> "" Then
            Try
                If connection = True Then
                    Cn.Open()
                End If
                Dim adapter As New SqlDataAdapter(SQLstring, Cn)
                adapter.SelectCommand.CommandTimeout = 90
                adapter.Fill(table)
                If connection = True Then
                    Cn.Close()
                End If
            Catch ex As System.Exception
                Cn.Close()
                If silent_try = False Then
                    MessageBox.Show("Es ist ein Fehler in der SQL-Abfrage aufgetreten:" & vbCr & ex.Message, "Fehler")
                End If

            End Try
        End If
        Return table
    End Function
End Module


Public Class Form1

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        createxml()
    End Sub


End Class

Public Class Creditor
    Public Vorname As String
    Public Nachname As String
    Public BIC As String
    Public IBAN As String
    Public Index As Integer
    Public Amount As Double

    Public Sub New(ByVal vnm As String, ByVal nnm As String, ByVal bc As String, ByVal ib As String, ByVal ind As Integer, ByVal am As Double)
        Vorname = vnm
        Nachname = nnm
        BIC = bc
        IBAN = ib
        Index = ind
        Amount = am
    End Sub
End Class