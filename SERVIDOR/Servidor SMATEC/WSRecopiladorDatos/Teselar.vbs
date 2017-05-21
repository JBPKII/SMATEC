' Set your settings
    strFileURL = "http://localhost:81/Service1.svc/teselar/TeselarData?"

' Fetch the file
    Set objXMLHTTP = CreateObject("MSXML2.XMLHTTP")

    objXMLHTTP.open  "GET", strFileURL, false
    objXMLHTTP.send()
    
If objXMLHTTP.Status = 200 Then
End if

Set objXMLHTTP = Nothing