#!/usr/bin/env python3
"""
Generate Markdown documentation from _events.yaml
"""

import yaml
import json
import os
from pathlib import Path

def load_events():
    """Load events from YAML file"""
    with open('_events.yaml', 'r') as f:
        return yaml.safe_load(f)

def generate_event_markdown(event, exchange_info):
    """Generate Markdown documentation for a single event"""
    md = []
    
    # Header
    md.append(f"# {event['name']}{event['version'].upper()}")
    md.append("")
    
    # Metadata table
    md.append("## Event Metadata")
    md.append("")
    md.append("| Property | Value |")
    md.append("|----------|-------|")
    md.append(f"| **Routing Key** | `{event['routing_key']}` |")
    md.append(f"| **Contract** | `{event['contract']}` |")
    md.append(f"| **Domain** | {event['domain']} |")
    md.append(f"| **Version** | {event['version']} |")
    md.append(f"| **Exchange** | `{exchange_info['exchange']}` |")
    md.append(f"| **Content-Type** | `{exchange_info['content_types'][0]}` (preferred), `{exchange_info['content_types'][1]}` (fallback) |")
    md.append("")
    
    # Description
    md.append("## Description")
    md.append("")
    md.append(event['description'])
    md.append("")
    
    # Characteristics
    md.append("## Characteristics")
    md.append("")
    md.append(f"- **Frequency**: {event['frequency']}")
    md.append(f"- **Idempotent**: {'Yes' if event['idempotent'] else 'No'}")
    md.append("")
    
    # Headers
    md.append("## Headers")
    md.append("")
    md.append("### Required Headers")
    md.append("- `x-event-id`: Unique event identifier (GUID/ULID)")
    md.append("- `x-event-ts`: Event timestamp (RFC3339 UTC)")
    md.append(f"- `x-contract`: `{event['contract']}`")
    md.append("")
    md.append("### Optional Headers")
    md.append("- `x-profile`: Player profile identifier (when applicable)")
    md.append("- `x-correlation-id`: For tracing related events")
    md.append("- `x-source`: Source system identifier")
    md.append("")
    
    # Schema
    md.append("## Schema")
    md.append("")
    md.append("| Field | Type | Required | Description |")
    md.append("|-------|------|----------|-------------|")
    
    for field_name, field_info in event['schema'].items():
        required = "Yes" if field_info['required'] else "No"
        description = field_info.get('description', '')
        md.append(f"| `{field_name}` | {field_info['type']} | {required} | {description} |")
    
    md.append("")
    
    # Example
    md.append("## Example")
    md.append("")
    md.append("### JSON Payload")
    md.append("```json")
    md.append(json.dumps(event['example'], indent=2))
    md.append("```")
    md.append("")
    
    # MessagePack note
    md.append("### MessagePack")
    md.append("When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.")
    md.append("")
    
    # Usage notes
    md.append("## Usage Notes")
    md.append("")
    md.append("### Publishing")
    md.append("```csharp")
    md.append(f"// Publish to: {event['routing_key']}")
    md.append(f"var contract = new {event['contract'].split('.')[-1]}");
    md.append("{")
    
    # Add required fields from example
    for field_name, value in event['example'].items():
        if isinstance(value, str):
            md.append(f'    {field_name} = "{value}",')
        else:
            md.append(f'    {field_name} = {value},')
    
    md.append("};")
    md.append(f'await publisher.PublishAsync("{event["routing_key"]}", contract);')
    md.append("```")
    md.append("")
    
    # Consuming
    md.append("### Consuming")
    md.append("```csharp")
    md.append("// Bind queue to routing key")
    md.append(f'channel.QueueBind(queue: "your-queue", exchange: "{exchange_info["exchange"]}", routingKey: "{event["routing_key"]}");')
    md.append("")
    md.append("// Or use wildcard patterns:")
    domain = event['domain'].rstrip('s')  # Remove trailing 's' from domain
    md.append(f'// - All {event["domain"]} events: "albion.event.{domain}.*.v1"')
    md.append('// - All V1 events: "albion.event.*.*.v1"')
    md.append('// - All events: "albion.event.#"')
    md.append("```")
    md.append("")
    
    # Related events
    md.append("## Related Events")
    md.append("")
    md.append(f"- Other {event['domain']} domain events")
    md.append("- See [Event Overview](../00-overview.md) for all available events")
    md.append("")
    
    # Version history
    md.append("## Version History")
    md.append("")
    md.append(f"- **{event['version']}** - Initial version")
    md.append("- See [CHANGELOG](../../messaging/CHANGELOG_EVENTS.md) for detailed changes")
    md.append("")
    
    return '\n'.join(md)

def main():
    """Main function to generate all documentation"""
    # Load events
    data = load_events()
    
    # Create domain directories if they don't exist
    domains = set()
    for event in data['events']:
        domains.add(event['domain'])
    
    for domain in domains:
        Path(domain).mkdir(exist_ok=True)
    
    # Generate documentation for each event
    for event in data['events']:
        # Determine file path
        event_name = event['name'].lower()
        version = event['version']
        domain = event['domain']
        filename = f"{domain}/{event_name}.{version}.md"
        
        # Generate markdown
        markdown = generate_event_markdown(event, data)
        
        # Write file
        with open(filename, 'w') as f:
            f.write(markdown)
        
        print(f"Generated: {filename}")
    
    print(f"\nGenerated documentation for {len(data['events'])} events")

if __name__ == "__main__":
    main()