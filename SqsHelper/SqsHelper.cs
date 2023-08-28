using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using SqsHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqsHelper
{

    public class SqsHelper : ISqsHelper
    {
        private readonly IAmazonSQS _sqs;
        private string QueueUrl { get; set; }
        public string UniqueKey { get; set; }

        public SqsHelper(AwsSqsOptions options, AWSCredentials credentials, RegionEndpoint region)
        {
            _sqs = new AmazonSQSClient(credentials, region);
            QueueUrl = options.QueueUrl;
            UniqueKey = options.UniqueKey;
        }

        public async Task<ReceiveMessageResponse> ReceiveMessagesAsync(
            bool deleteAfterRead,
            int maxNumberOfMessages,
            int WaitTimeSeconds = 0)
        {
            var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = QueueUrl,
                MaxNumberOfMessages = maxNumberOfMessages,
                WaitTimeSeconds = WaitTimeSeconds
            });

            if (deleteAfterRead && response.Messages.Count > 0)
            {
                await DeleteMessages(response.Messages);
            }

            return response;
        }

        public async Task<SendMessageResponse> SendMessageAsync(string messageBody)
        {
            var response = await _sqs.SendMessageAsync(QueueUrl, messageBody);
            return response;
        }

        public async Task<SendMessageResponse> SendMessageWithAttributesAsync(
            string messageBody,
            Dictionary<string, MessageAttributeValue> attributeValues
            )
        {
            var response = await _sqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = QueueUrl,
                MessageBody = messageBody,
                MessageAttributes = attributeValues
            });
            return response;
        }

        public async Task<SendMessageBatchResponse> SendMessageBatchAsync(
            IEnumerable<SendMessageBatchRequestEntry> messages
            )
        {
            var response = await _sqs.SendMessageBatchAsync(QueueUrl, messages.ToList());
            return response;
        }

        public async Task<PurgeQueueResponse> DeleteAllMessages()
        {
            var response = await _sqs.PurgeQueueAsync(QueueUrl);
            return response;
        }

        public async Task DeleteMessages(List<Message> messages)
        {
            var deleteMessageTasks = new List<Task>();
            foreach (var message in messages)
            {
                deleteMessageTasks.Add(
                    _sqs.DeleteMessageAsync(QueueUrl, message.ReceiptHandle)
                );
            }

            await Task.WhenAll(deleteMessageTasks);
        }

        public async Task DeleteBatchMessages(List<Message> messages)
        {
            var loopCount = (int)Math.Ceiling((double)messages.Count / 10);
            for (var i = 0; i < loopCount; i++)
            {
                var entries = messages
                    .Select(message => new DeleteMessageBatchRequestEntry(message.MessageId, message.ReceiptHandle))
                    .Skip(i * 10).Take(10).ToList();

                var response = await _sqs.DeleteMessageBatchAsync(new DeleteMessageBatchRequest
                {
                    QueueUrl = QueueUrl,
                    Entries = entries
                });
            }
        }

        public async Task<int> NumberOfMessagesInQueue()
        {
            var response = await _sqs.GetQueueAttributesAsync(new GetQueueAttributesRequest
            {
                QueueUrl = QueueUrl,
                AttributeNames = new List<string> { SqsConstants.ApproximateNumberOfMessages }
            });

            return response.ApproximateNumberOfMessages;
        }

    }
}