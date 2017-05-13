CREATE TABLE [dbo].[threads] (
    [threadid]   BIGINT        IDENTITY (1, 1) NOT NULL,
    [title]      VARCHAR (100) NOT NULL,
    [content]    TEXT          NOT NULL,
    [creator_id] BIGINT        NULL,
    [ctime]      DATETIME      DEFAULT (getdate()) NOT NULL,
    [mtime]      DATETIME      DEFAULT (getdate()) NOT NULL,
    [topicid]    INT           NOT NULL,
    [attachid]   BIGINT        DEFAULT (NULL) NULL,
    PRIMARY KEY CLUSTERED ([threadid] ASC),
    FOREIGN KEY ([attachid]) REFERENCES [dbo].[attachments] ([attachid]),
    FOREIGN KEY ([creator_id]) REFERENCES [dbo].[users] ([uid]),
    FOREIGN KEY ([topicid]) REFERENCES [dbo].[topics] ([topicid])
);

