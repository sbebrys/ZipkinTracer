namespace ZipkinTracer.Internal
{
	public static class TraceKeys
	{
		/// <summary>
		/// The client sent ("cs") a request to a server. There is only one send per
		/// span. For example, if there's a transport error, each attempt can be logged
		/// as a WIRE_SEND annotation.
		/// 
		/// If chunking is involved, each chunk could be logged as a separate
		/// CLIENT_SEND_FRAGMENT in the same span.
		/// 
		/// Annotation.host is not the server. It is the host which logged the send
		/// event, almost always the client. When logging CLIENT_SEND, instrumentation
		/// should also log the SERVER_ADDR.
		/// </summary>
		public const string ClientSend = "cs";

		/// <summary>
		/// The client received ("cr") a response from a server. There is only one
		/// receive per span. For example, if duplicate responses were received, each
		/// can be logged as a WIRE_RECV annotation.
		/// 
		/// If chunking is involved, each chunk could be logged as a separate
		/// CLIENT_RECV_FRAGMENT in the same span.
		/// 
		/// Annotation.host is not the server. It is the host which logged the receive
		/// event, almost always the client. The actual endpoint of the server is
		/// recorded separately as SERVER_ADDR when CLIENT_SEND is logged.
		/// </summary>
		public const string ClientRecv = "cr";

		/// <summary>
		/// The server sent ("ss") a response to a client. There is only one response
		/// per span. If there's a transport error, each attempt can be logged as a
		/// WIRE_SEND annotation.
		/// 
		/// Typically, a trace ends with a server send, so the last timestamp of a trace
		/// is often the timestamp of the root span's server send.
		/// 
		/// If chunking is involved, each chunk could be logged as a separate
		/// SERVER_SEND_FRAGMENT in the same span.
		/// 
		/// Annotation.host is not the client. It is the host which logged the send
		/// event, almost always the server. The actual endpoint of the client is
		/// recorded separately as CLIENT_ADDR when SERVER_RECV is logged.
		/// </summary>
		public const string ServerSend = "ss";

		/// <summary>
		/// The server received ("sr") a request from a client. There is only one
		/// request per span.  For example, if duplicate responses were received, each
		/// can be logged as a WIRE_RECV annotation.
		/// 
		/// Typically, a trace starts with a server receive, so the first timestamp of a
		/// trace is often the timestamp of the root span's server receive.
		/// 
		/// If chunking is involved, each chunk could be logged as a separate
		/// SERVER_RECV_FRAGMENT in the same span.
		/// 
		/// Annotation.host is not the client. It is the host which logged the receive
		/// event, almost always the server. When logging SERVER_RECV, instrumentation
		/// should also log the CLIENT_ADDR.
		/// </summary>
		public const string ServerRecv = "sr";

		/// <summary>
		/// The domain portion of the URL or host header. Ex. "mybucket.s3.amazonaws.com"
		///
		/// Used to filter by host as opposed to ip address.
		/// </summary>
		public const string HttpHost = "http.host";

		/// <summary>
		/// The HTTP method, or verb, such as "GET" or "POST".
		///
		/// Used to filter against an http route.
		/// </summary>
		public const string HttpMethod = "http.method";

		/// <summary>
		/// The absolute http path, without any query parameters. Ex. "/objects/abcd-ff"
		///
		/// Used to filter against an http route, portably with zipkin v1.
		///
		/// In zipkin v1, only equals filters are supported. Dropping query parameters makes the number
		/// of distinct URIs less. For example, one can query for the same resource, regardless of signing
		/// parameters encoded in the query line. This does not reduce cardinality to a HTTP single route.
		/// For example, it is common to express a route as an http URI template like
		/// /resource/{resource_id}. In systems where only equals queries are available, searching for
		/// http/path=/resource won't match if the actual request was /resource/abcd-ff.
		///
		/// Historical note: This was commonly expressed as "http.uri" in zipkin, eventhough it was most
		/// often just a path.
		/// </summary>
		public const string HttpPath = "http.path";

		/// <summary>
		/// The entire URL, including the scheme, host and query parameters if available. Ex.
		/// "https://mybucket.s3.amazonaws.com/objects/abcd-ff?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Algorithm=AWS4-HMAC-SHA256..."
		/// Combined with {@linkplain #HTTP_METHOD}, you can understand the fully-qualified request line.
		/// This is optional as it may include private data or be of considerable length.
		/// </summary>
		public const string HttpUrl = "http.url";

		/// <summary>
		/// The HTTP status code, when not in 2xx range. Ex. "503"
		///
		/// Used to filter for error status.
		/// </summary>
		public const string HttpStatusCode = "http.status_code";

		/// <summary>
		/// The size of the non-empty HTTP request body, in bytes. Ex. "16384"
		///
		/// Large uploads can exceed limits or contribute directly to latency.
		/// </summary>
		public const string HttpRequestSize = "http.request.size";

		/// <summary>
		/// The size of the non-empty HTTP response body, in bytes. Ex. "16384"
		///
		/// Large downloads can exceed limits or contribute directly to latency.
		/// </summary>
		public const string HttpResponseSize = "http.response.size";

		/// <summary>
		/// The query executed for SQL call.  Ex. "select/// from customers where id = ?"
		/// Used to filter by SQL query.
		/// </summary>
		public const string SqlQuery = "sql.query";

		/// <summary>
		/// When an annotation value, this indicates when an error occurred. When a
		/// binary annotation key, the value is a human readable message associated
		/// with an error.
		/// 
		/// Due to transient errors, an ERROR annotation should not be interpreted
		/// as a span failure, even the annotation might explain additional latency.
		/// Instrumentation should add the ERROR binary annotation when the operation
		/// failed and couldn't be recovered.
		/// 
		/// Here's an example: A span has an ERROR annotation, added when a WIRE_SEND
		/// failed. Another WIRE_SEND succeeded, so there's no ERROR binary annotation
		/// on the span because the overall operation succeeded.
		/// 
		/// Note that RPC spans often include both client and server hosts: It is
		/// possible that only one side perceived the error.
		/// </summary>
		public const string Error = "error";

		/// <summary>
		/// Indicates a client address ("ca") in a span. Most likely, there's only one.
		/// Multiple addresses are possible when a client changes its ip or port within
		/// a span.
		/// </summary>
		public const string ClientAddr = "ca";

		/// <summary>
		/// Indicates a server address ("sa") in a span. Most likely, there's only one.
		/// Multiple addresses are possible when a client is redirected, or fails to a
		/// different server ip or port.
		/// </summary>
		public const string ServerAddr = "sa";

		/// <summary>
		/// The BinaryAnnotation#value value of "lc" is the component or namespace of a local
		/// span.
		///
		/// BinaryAnnotation#endpoint adds service context needed to support queries.
		///
		/// Local Component("lc") supports three key features: flagging, query by service and filtering
		/// Span.name by namespace.
		///
		/// While structurally the same, local spans are fundamentally different than RPC spans in how
		/// they should be interpreted.For example, zipkin v1 tools center on RPC latency and service
		/// graphs.Root local-spans are neither indicative of critical path RPC latency, nor have impact
		/// on the shape of a service graph. By flagging with "lc", tools can special-case local spans.
		///
		/// Zipkin v1 Spans are unqueryable unless they can be indexed by service name.The only path
		/// to a Endpoint#serviceName service name is via BinaryAnnotation#endpoint host. By logging "lc", a local span can be queried even if no other annotations are logged.
		///
		/// The value of "lc" is the namespace of Span#name. For example, it might be
		/// "finatra2", for a span named "bootstrap". "lc" allows you to resolves conflicts for the same
		/// Span.name, for example "finatra/bootstrap" vs "finch/bootstrap". Using local component, you'd
		/// search for spans named "bootstrap" where "lc=finch"
		/// </summary>
		public const string LocalComponent = "lc";
	}
}