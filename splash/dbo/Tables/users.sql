CREATE TABLE [dbo].[users] (
    [uid]      BIGINT        IDENTITY (1, 1) NOT NULL,
    [username] VARCHAR (50)  NOT NULL,
    [password] VARCHAR (50)  NOT NULL,
    [email]    VARCHAR (100) NOT NULL,
    [regtime]  DATETIME      DEFAULT (getdate()) NOT NULL,
    [modtime]  DATETIME      DEFAULT (getdate()) NOT NULL,
    [verified] TINYINT       DEFAULT ((0)) NOT NULL,
    [fname]    VARCHAR (50)  DEFAULT (NULL) NULL,
    [lname]    VARCHAR (50)  DEFAULT (NULL) NULL,
    [profpic]  BIGINT        DEFAULT (NULL) NULL,
    PRIMARY KEY CLUSTERED ([uid] ASC),
    FOREIGN KEY ([profpic]) REFERENCES [dbo].[attachments] ([attachid]),
    UNIQUE NONCLUSTERED ([email] ASC),
    UNIQUE NONCLUSTERED ([username] ASC)
);

