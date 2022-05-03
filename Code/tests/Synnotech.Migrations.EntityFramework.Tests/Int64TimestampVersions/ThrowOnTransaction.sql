DECLARE @num_transactions INT;
SET @num_transactions = (SELECT COUNT(*) FROM sys.sysprocesses WHERE open_tran = 1);
IF @num_transactions > 0
	THROW 50001, 'There is an open transaction although there should be none', 1;