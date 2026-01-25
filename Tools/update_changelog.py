#!/usr/bin/env python3

import sys
import os
import logging
from typing import List, Any, Optional
import yaml
import argparse
import datetime

MAX_ENTRIES = 500

HEADER_RE = r"(?::cl:|🆑) *\r?\n(.+)$"
ENTRY_RE = r"^ *[*-]? *(\S[^\n\r]+)\r?$"

CATEGORY_MAIN = "Main"

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stderr)  # Log to stderr for GitHub Actions
    ]
)
logger = logging.getLogger(__name__)

# From https://stackoverflow.com/a/37958106/4678631
class NoDatesSafeLoader(yaml.SafeLoader):
    @classmethod
    def remove_implicit_resolver(cls, tag_to_remove):
        """
        Remove an implicit resolver tag from a YAML loader class's implicit resolvers.
        
        This mutates the class-level `yaml_implicit_resolvers` mapping so any resolver entries whose tag equals `tag_to_remove` are removed for all first-letter keys. If the class does not yet have its own `yaml_implicit_resolvers` dict, a shallow copy of the existing mapping is created before modification to avoid altering parent classes' resolvers.
        
        Parameters:
            cls (type): YAML loader class (typically a subclass of `yaml.SafeLoader`) whose implicit resolvers will be modified.
            tag_to_remove (str): The resolver tag (e.g., `'tag:yaml.org,2002:timestamp'`) to remove from the implicit resolvers.
        """
        if not 'yaml_implicit_resolvers' in cls.__dict__:
            cls.yaml_implicit_resolvers = cls.yaml_implicit_resolvers.copy()

        for first_letter, mappings in cls.yaml_implicit_resolvers.items():
            cls.yaml_implicit_resolvers[first_letter] = [(tag, regexp)
                                                         for tag, regexp in mappings
                                                         if tag != tag_to_remove]

# Disable automatic timestamp parsing to avoid serialization issues
NoDatesSafeLoader.remove_implicit_resolver('tag:yaml.org,2002:timestamp')

def load_yaml_file(file_path: str) -> Optional[Any]:
    """
    Load and parse a YAML file using a loader that disables implicit timestamp parsing.
    
    Parameters:
        file_path (str): Path to the YAML file. The file is opened with UTF-8 with BOM support.
    
    Returns:
        The parsed YAML content (Python objects), or `None` if the file is empty.
    
    Raises:
        yaml.YAMLError: If the YAML content cannot be parsed.
        IOError: If the file cannot be read.
    """
    try:
        logger.debug(f"Attempting to load YAML file: {file_path}")
        with open(file_path, "r", encoding="utf-8-sig") as f:
            content = f.read()
            logger.debug(f"File read successfully, size: {len(content)} bytes")
            parsed = yaml.load(content, Loader=NoDatesSafeLoader)
            logger.debug(f"YAML parsed successfully")
            return parsed
    except yaml.YAMLError as e:
        logger.error(f"Failed to parse YAML file {file_path}: {e}")
        raise
    except IOError as e:
        logger.error(f"Failed to read file {file_path}: {e}")
        raise

def save_yaml_file(file_path: str, data: Any) -> None:
    """
    Write a Python object to a YAML file using UTF-8 with BOM and log success or failure.
    
    Parameters:
        file_path (str): Path to the output YAML file.
        data (Any): YAML-serializable object; if it contains an "Entries" list, its length is logged.
    
    Raises:
        IOError: If the file cannot be written.
        Exception: On unexpected errors during serialization or file operations.
    """
    try:
        logger.debug(f"Attempting to save YAML file: {file_path}")
        with open(file_path, "w", encoding="utf-8-sig") as f:
            yaml.safe_dump(data, f)
        logger.info(f"Successfully saved YAML file: {file_path}")
        logger.debug(f"Data structure saved: {len(data.get('Entries', []))} entries")
    except IOError as e:
        logger.error(f"Failed to write file {file_path}: {e}")
        raise
    except Exception as e:
        logger.error(f"Unexpected error while saving YAML file: {e}")
        raise

def process_changelog_part(part_path: str, category_filter: str, current_max_id: int) -> Optional[dict]:
    """
    Convert a changelog part YAML file into a normalized changelog entry when it matches the given category.
    
    Parameters:
        part_path (str): Filesystem path to the changelog part YAML file.
        category_filter (str): Category name required for the part to be accepted.
        current_max_id (int): Highest existing entry ID; used to compute the new entry's `id`.
    
    Returns:
        dict: A new entry with keys `author`, `time` (ISO 8601 string), `changes` (list), `id` (int), and optional `url` when the part is accepted and valid.
        None: If the part is empty, invalid, missing required fields, has no changes, its category does not match `category_filter`, or an error occurs during processing.
    """
    logger.info(f"Processing changelog part: {part_path}")
    
    try:
        partyaml = load_yaml_file(part_path)
        if not partyaml:
            logger.warning(f"Part file {part_path} is empty or invalid")
            return None
        
        # Validate required fields
        required_fields = ['author', 'changes']
        for field in required_fields:
            if field not in partyaml:
                logger.error(f"Part file {part_path} missing required field: {field}")
                return None
        
        # Check category
        part_category = partyaml.get("category", CATEGORY_MAIN)
        if part_category != category_filter:
            logger.info(f"Skipping {part_path}: category mismatch ({part_category} vs {category_filter})")
            return None
        
        # Prepare entry data
        author = partyaml["author"]
        time = partyaml.get(
            "time", 
            datetime.datetime.now(datetime.timezone.utc).isoformat()
        )
        changes = partyaml["changes"]
        url = partyaml.get("url")
        
        # Normalize changes to list
        if not isinstance(changes, list):
            changes = [changes]
        
        logger.debug(f"Part details - Author: {author}, Time: {time}, "
                    f"Changes count: {len(changes)}, URL: {url}")
        
        if len(changes) == 0:
            logger.warning(f"Part {part_path} has no changes, skipping")
            return None
        
        # Create new entry
        new_id = current_max_id + 1
        entry = {
            "author": author,
            "time": time,
            "changes": changes,
            "id": new_id,
            "url": url
        }
        
        logger.info(f"Created new changelog entry with ID: {new_id}")
        return entry
        
    except Exception as e:
        logger.error(f"Error processing part {part_path}: {e}")
        return None

def main():
    """
    Merge YAML changelog part files from a directory into the main changelog YAML file and save the updated changelog.
    
    Processes every `.yml` file in the configured parts directory, converts valid part files into new changelog entries (filtered by category), appends them to the existing "Entries" list (or creates one if missing), removes successfully processed part files, enforces a maximum number of entries by dropping the oldest when necessary, preserves other top-level metadata keys from the original changelog, and writes the resulting data back to the main changelog file. Exits the process if the provided changelog file or parts directory does not exist.
    """
    parser = argparse.ArgumentParser(
        description="Merge changelog parts into main changelog YAML file."
    )
    parser.add_argument("changelog_file", help="Path to main changelog YAML file")
    parser.add_argument("parts_dir", help="Directory containing changelog part files")
    parser.add_argument("--category", default=CATEGORY_MAIN,
                       help=f"Category filter for parts (default: {CATEGORY_MAIN})")
    
    args = parser.parse_args()
    
    logger.info(f"Starting changelog processing")
    logger.info(f"Changelog file: {args.changelog_file}")
    logger.info(f"Parts directory: {args.parts_dir}")
    logger.info(f"Category filter: {args.category}")
    
    # Validate inputs
    if not os.path.exists(args.changelog_file):
        logger.error(f"Changelog file not found: {args.changelog_file}")
        sys.exit(1)
    
    if not os.path.exists(args.parts_dir):
        logger.error(f"Parts directory not found: {args.parts_dir}")
        sys.exit(1)
    
    # Load current changelog data
    logger.info("Loading current changelog data")
    current_data = load_yaml_file(args.changelog_file)
    
    if current_data is None:
        logger.info("No existing changelog data found, initializing new structure")
        entries_list = []
        current_data = {}
    else:
        entries_list = current_data.get("Entries", [])
        logger.info(f"Loaded {len(entries_list)} existing entries")
    
    # Calculate current maximum ID
    max_id = max(map(lambda e: e["id"], entries_list), default=0)
    logger.debug(f"Current maximum entry ID: {max_id}")
    
    # Process each part file
    processed_count = 0
    skipped_count = 0
    error_count = 0
    
    part_files = [f for f in os.listdir(args.parts_dir) if f.endswith('.yml')]
    logger.info(f"Found {len(part_files)} YAML part files to process")
    
    for partname in part_files:
        partpath = os.path.join(args.parts_dir, partname)
        
        try:
            new_entry = process_changelog_part(partpath, args.category, max_id)
            
            if new_entry:
                entries_list.append(new_entry)
                max_id = new_entry["id"]  # Update max_id for next iteration
                processed_count += 1
                
                # Remove processed part file
                try:
                    os.remove(partpath)
                    logger.debug(f"Removed processed part file: {partpath}")
                except OSError as e:
                    logger.warning(f"Failed to remove part file {partpath}: {e}")
            else:
                skipped_count += 1
                
        except Exception as e:
            logger.error(f"Failed to process part file {partname}: {e}")
            error_count += 1
    
    logger.info(f"Processing complete - Added: {processed_count}, "
                f"Skipped: {skipped_count}, Errors: {error_count}")
    
    # Log current state
    logger.info(f"Total entries before cleanup: {len(entries_list)}")
    
    # Sort entries by ID
    entries_list.sort(key=lambda e: e["id"])
    logger.debug("Entries sorted by ID")
    
    # Apply overflow limit
    overflow = len(entries_list) - MAX_ENTRIES
    if overflow > 0:
        logger.info(f"Removing {overflow} old entries (limit: {MAX_ENTRIES})")
        entries_list = entries_list[overflow:]
        logger.info(f"Remaining entries after cleanup: {len(entries_list)}")
    else:
        logger.debug(f"No overflow detected (current: {len(entries_list)}, limit: {MAX_ENTRIES})")
    
    # Prepare new data structure
    new_data = {"Entries": entries_list}
    
    # Copy all other keys from original data
    original_keys = [k for k in current_data.keys() if k != "Entries"]
    for key in original_keys:
        new_data[key] = current_data[key]
        logger.debug(f"Preserved metadata key: {key}")
    
    # Save updated changelog
    logger.info(f"Saving updated changelog with {len(entries_list)} entries")
    save_yaml_file(args.changelog_file, new_data)
    
    # Summary
    logger.info("Changelog update completed successfully")
    logger.info(f"Final statistics: Total entries: {len(entries_list)}, "
                f"Processed: {processed_count}, Skipped: {skipped_count}, Errors: {error_count}")

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        logger.info("Process interrupted by user")
        sys.exit(1)
    except Exception as e:
        logger.error(f"Unhandled exception in main: {e}")
        sys.exit(1)