<!DOCTYPE html>
<html>
<head>
	<title>WebReports</title>
	<style type="text/css">
		body, html {
			width: 100%;
			height: 100%;
			padding: 0;
			margin: 0;
		}

		table {
    		border-collapse: collapse;
		}

		td {
    		position: relative;
    		padding: 5px 10px;
		}

		form {
			height: 150px;
			width: 500px;
			color: black;
			border-radius: 20px;
			border: 5px #1A1A1A solid;
			font-family: Garamond;
			position: absolute;
			top: 50%;
			left: 50%;
			margin-top: -100px;
			margin-left: -250px;
			background: white;
			text-align: center;
			padding: 20px;
		}

		#error {
			background: red;
			padding: 20px;
			display: block;
			text-align: center;
			font-family: Garamond;
			font-weight: bold;
			font-size: 20px;
		}
	</style>
	<script type="text/javascript" src="http://code.jquery.com/jquery-2.2.0.min.js"></script>
	<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.5.0/css/font-awesome.min.css">
	<script type="text/javascript">
    	window.cookieconsent_options = {"message":"This website uses cookies to ensure you get the best experience on our website","dismiss":"Got it!","learnMore":"More info","link":"?cookies=true","theme":"dark-bottom"};
	</script>

	<script type="text/javascript" src="//cdnjs.cloudflare.com/ajax/libs/cookieconsent2/1.0.9/cookieconsent.min.js"></script>
</head>
<body>
	<?php 
	include "config.php";
	if (isset($_GET["cookies"])) {
				die('This page use cookies to remember:
						<ul><li>interval for refreshing page</li>
						<li>if you have agreed (or not) to our use of cookies on this site</li></ul>
						Enabling these cookies is not strictly necessary for the website to work but it will provide you with a better browsing experience. You can delete or block these cookies, but if you do that some features of this site may not work as intended.
						The cookie-related information is not used to identify you personally and the pattern data is fully under our control. These cookies are not used for any purpose other than those described here.
						<script>
							$(function() {
								var timeout = setTimeout(function() {
									$("a.cc_btn.cc_btn_accept_all").removeAttr("href");
									$(".cc_btn_accept_all").click(function() {window.location = window.location.href.split("?")[0];});
								}, 1000);
							});
						</script>
					');
			}
	?>

	<?php 
$loginPage = '
<body>
	<form action="" method="post">
		<font style="font-size: 25px; font-weight: bold">Please login to manage reports on your server<br><br>
		Your username: <input type="text" name="username"><br>
		Your password: <input type="password" name="password"><br>
		<input type="submit" value="Login!">
		</font>
	</form>
</body>';

$errorPage = '
<body>
	<div id="error">Password or username was incorrect! Try again.</div>
	<form action="" method="post">
		<font style="font-size: 25px; font-weight: bold">Please login to manage reports on your server<br><br>
		Your username: <input type="text" name="username"><br>
		Your password: <input type="password" name="password"><br>
		<input type="submit" value="Login!">
		</font>
	</form>
</body>';
	error_reporting(0);
	if (isset($_POST["username"])) {
		foreach ($logins as $user => $userdata) {
			if ($user == $_POST["username"]) {
				if ($userdata == $_POST["password"]) {
					goto end;
				}
			}
		}
		die($errorPage);
	} elseif ($_COOKIE["loggedin"] == true){
		goto end;
	} else {
		die($loginPage);
	}

	end:
	?>



	<div style="text-align: center; width: 100%; margin-top: 5px;">Auto update? <input checked type="checkbox" id="autoupdate">&nbsp; Interval <input type="number" id="autoupdate_time" style="width:40px" value="<?php if(isset($_COOKIE["time"])) {echo $_COOKIE["time"];} else {echo "10";}?>"> s</div>
	<table border="2px solid black" style="display: block; position: absolute; top: 30px;" id="table">
		<tr style="text-align: center">
			<td>&nbsp;&nbsp;&nbsp;Report ID&nbsp;&nbsp;&nbsp;</td>
			<td>&nbsp;&nbsp;&nbsp;Reported by&nbsp;&nbsp;&nbsp;</td>
			<td>&nbsp;&nbsp;&nbsp;Reported player&nbsp;&nbsp;&nbsp;</td>
			<td>&nbsp;&nbsp;&nbsp;Reason&nbsp;&nbsp;&nbsp;</td>
			<td>&nbsp;&nbsp;&nbsp;Solved&nbsp;&nbsp;&nbsp;</td>
			<td>&nbsp;&nbsp;&nbsp;Action&nbsp;&nbsp;&nbsp;</td>
		</tr>
		<?php 
			header("Content-Type: text/html;charset=UTF-8");
			$mysql = mysqli_connect($DatabaseAddress, $DatabaseUsername , $DatabasePassword, $DatabaseName);
			mysqli_query($mysql, "SET NAMES 'utf8'");
			if (isset($_GET["delete"])) {
				$query = "DELETE FROM $DatabaseTableName WHERE $DatabaseTableName.ID = $_GET[delete]";
				mysqli_query($mysql, $query);
			}

			if (isset($_GET["check"])) {
				$query = "UPDATE $DatabaseTableName SET COMPLETED = '1' WHERE $DatabaseTableName.ID = $_GET[check];";
				mysqli_query($mysql, $query);
			}

			if (isset($_GET["uncheck"])) {
				$query = "UPDATE $DatabaseTableName SET COMPLETED = '0' WHERE $DatabaseTableName.ID = $_GET[uncheck];";
				mysqli_query($mysql, $query);
			}

			$query = "SELECT ID, ReportedBy, Reported, Reason, Completed FROM $DatabaseTableName";
			$result = mysqli_query($mysql, $query);
			while ($row = mysqli_fetch_assoc($result)) {
				if ($row["Completed"] == 1) {
					echo "<tr style='color: gray;'>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["ID"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["ReportedBy"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["Reported"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["Reason"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo "Solved";
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td style='text-align: center;' class='action'>";
					echo "<a style='color: black' href='?delete=$row[ID]' title='Delete report'><i class='fa fa-trash-o'></i></a>&nbsp;&nbsp;<a style= 'color: black' href='?uncheck=$row[ID]' title='Mark unsolved'><i class='fa fa-times'></i></a>";
					echo "</td>";
					echo "</tr>";
				} else {	
					echo "<tr>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["ID"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["ReportedBy"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["Reported"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo $row["Reason"];
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td>&nbsp;&nbsp;&nbsp;";
					echo "Not solved";
					echo "&nbsp;&nbsp;&nbsp;</td>";
					echo "<td style='text-align: center;' class='action'>";
					echo "<a style= 'color: black' href='?delete=$row[ID]' title='Delete report'><i class='fa fa-trash-o'></i></a>&nbsp;&nbsp;<a style= 'color: black' href='?check=$row[ID]' title='Mark solved'><i class='fa fa-check'></i></a>";
					echo "</td>";
					echo "</tr>";
				}
			}
		?>
	</table>
	<script type="text/javascript">
		var interval = "";
		document.cookie = "loggedin=true";

		function resize() {
			number = document.body.offsetWidth / 2;
			number = number - document.getElementById("table").offsetWidth / 2;
			document.getElementById("table").style.left = number + "px";
		}
		resize();

		$(window).resize(function() {resize();});
		$(function() {
			var time = document.getElementById("autoupdate_time").value * 1000;
			interval = setInterval(function() {
				if($("#autoupdate").is(":checked")) {
					window.location = window.location.href.split("?")[0];
					document.cookie = "time=" + document.getElementById("autoupdate_time").value;
				}
			}, time);
		})

		$("#autoupdate_time").on("change", function() {
			clearInterval(interval);
			var time = document.getElementById("autoupdate_time").value * 1000;
			interval = setInterval(function() {
				if($("#autoupdate").is(":checked")) {
					window.location = window.location.href.split("?")[0];
					document.cookie = "time=" + document.getElementById("autoupdate_time").value;
				}
			}, time);
		})
	</script>
</body>
</html>