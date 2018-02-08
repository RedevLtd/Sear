## Simple ElasticSearch Alerting by Redev

Sear is a bare bones application to provide simple alerting for ElasticSearch. It simply queries your ES data, as defined in an Alert config, when the query hits are higher or lower than the specified threshold an action is performed.

Currently only Slack is supported, but new action can be easily added.

## Example Configuration

Below is an example Sear Alert config, the Query is sent to your ES server as define.

	<Alert>
		<Name>SMS Errors</Name>
		 <Interval>* * * * *</Interval>
		 <Host>http://localhost:9200</Host>
		 <Index>logstash-*</Index>
		 <Query>
			{
			"query": {
			  "bool": {
				"must": [
						  {
							"query_string": {
							"query": "(ExceptionObject.Message:\"Failed to send the SMS\"",
							"analyze_wildcard": true,
							"default_field": "*"
							}
						  },
						  {
							"range": {
							"@timestamp": {
							"gte": "now-10m",
							"lte": "now"
							}
						  }
						}
					  ],
				"filter": [],
				"should": [],
				"must_not": []
				}
			  }
			}
		 </Query>
		 <Hits>9</Hits>
		 <HitType>Higher</HitType>
		 <ActionType>Slack</ActionType>
		 <ActionConfig>https://hooks.slack.com/services/<SlackHookToken></ActionConfig>
		 <Link>http://linktoyourviz.co.uk</Link>  
	</Alert>
